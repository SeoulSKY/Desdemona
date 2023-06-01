mod board;
mod errors;
mod game;
mod bot;

#[macro_use] extern crate rocket;

use crate::board::Board;
use crate::bot::Bot;
use crate::game::{Game, Player};

#[get("/")]
fn index() -> &'static str {
    "Hello, world!"
}

#[get("/decide?<board>&<intelligence>")]
fn decide(board: String, intelligence: u32) -> String {
    let mut bot = Bot::new(intelligence);
    let board = serde_json::from_str::<Board>(board.as_str()).unwrap();
    
    let action = bot.decide(Game::parse(board, Player::Bot));
    serde_json::to_string(&action).unwrap()
}

#[rocket::main]
async fn main() -> Result<(), rocket::Error> {
    rocket::build()
        .mount("/", routes![index, decide])
        .launch()
        .await?;

    Ok(())
}
