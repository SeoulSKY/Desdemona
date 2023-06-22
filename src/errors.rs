#[derive(Debug)]
pub enum Error {
    InvalidArgument(String),
    ParseError(String)
}
