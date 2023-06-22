mod board;
mod errors;
mod game;
mod bot;

#[macro_use] extern crate rocket;

use itertools::Itertools;
use rocket::response::status::BadRequest;
use serde_json::{json, Value};
use crate::board::Board;
use crate::bot::Bot;
use crate::game::{Game, Player};

#[get("/")]
fn index() -> &'static str {
    "Hello World!"
}

#[get("/actions?<board>&<current_player>")]
fn actions(board: String, current_player: String) -> Result<String, BadRequest<String>> {
    let board = Board::parse(board);
    if board.is_err() {
        return Err(BadRequest(Some("Invalid board".to_string())));
    }
    
    let current_player = Player::parse(current_player.chars().next().unwrap());
    if current_player.is_err() {
        return Err(BadRequest(Some("Invalid player".to_string())));
    }
    
    let current_player = current_player.unwrap();
    let game = Game::parse(board.unwrap(), current_player.clone());
    Ok(Value::Array(
        game.actions(current_player)
            .map(|a| Value::String(a.to_string()))
            .collect_vec()
    ).to_string())
}

#[get("/decide?<board>&<intelligence>")]
fn decide(board: String, intelligence: u32) -> Result<String, BadRequest<String>> {
    let mut bot = Bot::new(intelligence);
    let board = Board::parse(board);
    if board.is_err() {
        return Err(BadRequest(Some("Invalid board".to_string())));
    }
    
    let decision = bot.decide(Game::parse(board.unwrap(), Player::Bot));
    if decision.is_err() {
        return Err(BadRequest(Some("No actions are available for the AI from the given board".to_string())));
    }
    
    let (action, game) = decision.unwrap();
    
    let mut json = json!({
        "decision": action.to_string(),
        "result": game.board().to_string(),
    });
    
    if game.is_over() {
        json["winner"] = serde_json::to_value(game.winner().map(|p| p.disk().to_string()))
            .unwrap_or_else(|_| Value::Null);
    }
    
    Ok(json.to_string())
}

#[rocket::main]
async fn main() -> Result<(), rocket::Error> {
    rocket::build()
        .mount("/", routes![index, actions, decide])
        .launch()
        .await?;

    Ok(())
}
