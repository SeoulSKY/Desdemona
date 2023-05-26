mod board;
mod errors;
mod game;
mod bot;
mod api;

#[macro_use] extern crate rocket;

#[get("/")]
fn index() -> &'static str {
    "Hello, world!"
}

#[rocket::main]
async fn main() -> Result<(), rocket::Error> {
    rocket::build()
        .mount("/", routes![index])
        .launch()
        .await?;

    Ok(())
}
