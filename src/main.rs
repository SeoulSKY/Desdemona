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

// #[get("/decide?<board>&<intelligence>")]
// fn decide(board: String, intelligence: u32) -> &'static str {
//     let mut bot = Bot::new(intelligence);
//     
//     let action = bot.decide(Game::parse(Board::parse(board).unwrap(), Player::Bot));
//     ""
// }

#[rocket::main]
async fn main() -> Result<(), rocket::Error> {
    rocket::build()
        .mount("/", routes![index])
        .launch()
        .await?;

    Ok(())
}
