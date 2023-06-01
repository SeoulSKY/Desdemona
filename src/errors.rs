#[derive(Debug)]
pub enum BoardError {
    InvalidArgument(String),
    ParseError(String)
}
