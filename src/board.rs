use std::fmt::{Display, Formatter};
use crate::board::Disk::{Dark, Light};
use crate::errors::BoardError;
use crate::errors::BoardError::InvalidArgument;
use Direction::{North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest};


const BOARD_SIZE: usize = 8;


#[derive(Copy, Clone, PartialEq, Debug)]
pub enum Direction {
    North,
    NorthEast,
    East,
    SouthEast,
    South,
    SouthWest,
    West,
    NorthWest,
}

impl Direction {
    
    /// Returns the iterator for all possible directions
    pub fn all() -> impl Iterator<Item=Direction> {
       vec![North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest].into_iter()
    }
}


#[derive(Clone, Copy, PartialEq, Debug)]
pub enum Disk {
    Dark,
    Light,
}

impl Display for Disk {
    fn fmt(&self, f: &mut Formatter<'_>) -> std::fmt::Result {
        match self {
            Dark => write!(f, "D"),
            _ => write!(f, "L"),
        }
    }
}

impl Disk {
    
    /// Returns the opposite disk of this disk
    pub(crate) fn opponent(&self) -> Self {
        match *self {
            Dark => Light,
            Light => Dark,
        }
    }
}

#[derive(Debug, PartialEq, Clone)]
pub struct Position {
    row: usize,
    col: usize,
}

impl Position {
    
    /// Creates a new Position
    fn new(row: usize, col: usize) -> Self {
        Self {
            row,
            col,
        }
    }
    
    /// Checks if this position is in bound
    fn is_inbound(&self) -> bool {
        self.row < BOARD_SIZE && self.col < BOARD_SIZE
    }
    
    /// Returns the direction towards the target
    /// Pre-conditions:
    /// * self != target
    pub fn direction(&self, target: &Position) -> Direction {
        assert_ne!(self, target);
        
        let row_diff = target.row as i32 - self.row as i32;
        let hor_diff = target.col as i32 - self.col as i32;
        
        if row_diff < 0 {
            if hor_diff < 0 {
                NorthWest
            } else if hor_diff == 0 {
                North
            } else {
                NorthEast
            }
        } else if row_diff == 0 {
            if hor_diff < 0 {
                West
            } else {
                East
            }
        } else {
            if hor_diff < 0 {
                SouthWest
            } else if hor_diff == 0 {
                South
            } else {
                SouthEast
            } 
        }
    }
}

impl Display for Position {
    
    fn fmt(&self, f: &mut Formatter<'_>) -> std::fmt::Result {
        static ALPHABETS: &str = "ABCDEFGH";

        write!(f, "{}{}", ALPHABETS.chars().nth(self.col).unwrap(), self.row + 1)
    }
}

pub struct Board {
    grid: [[Option<Disk>; BOARD_SIZE]; BOARD_SIZE],
}

impl Display for Board {
    fn fmt(&self, f: &mut Formatter<'_>) -> std::fmt::Result {
        let mut buf = String::with_capacity(BOARD_SIZE * BOARD_SIZE + BOARD_SIZE);
        
        for row in self.grid.iter() {
            for disk in row.iter() {
                buf.push(match disk {
                    None => 'E',
                    Some(disk) => disk.to_string().chars().nth(0).unwrap(),
                })
            }
            
            buf.push('\n');
        }
        
        write!(f, "{}", buf)
    }
}

impl Board {
    
    /// Creates a new board
    pub fn new() -> Self {
        assert_eq!(BOARD_SIZE % 2, 0, "Board size must be even");
        
        let mut board = Board {
            grid: [[None; BOARD_SIZE]; BOARD_SIZE]
        };
        
        let mid_pos = Position::new(BOARD_SIZE / 2 - 1, BOARD_SIZE / 2 - 1);
        
        board.grid[mid_pos.row][mid_pos.col] = Some(Dark);
        board.grid[mid_pos.row + 1][mid_pos.col] = Some(Light);
        board.grid[mid_pos.row][mid_pos.col + 1] = Some(Light);
        board.grid[mid_pos.row + 1][mid_pos.col + 1] = Some(Dark);

        board
    }
    
    /// Returns the disk at the given position
    pub fn disk_at(&self, pos: &Position) -> Option<Disk> {
        self.grid[pos.row][pos.col]
    }
    
    /// Places the disk at the given position
    /// Pre-conditions:
    /// * Given position isn't occupied by a disk
    pub fn place(&mut self, disk: Disk, pos: &Position) -> Result<(), BoardError> {
        if self.disk_at(pos).is_some() {
            return Err(InvalidArgument(
                format!("Given position is not empty to place a disk: {}", pos)));
        }
        
        self.grid[pos.row][pos.col] = Some(disk);
        Ok(())
    }
    
    /// Returns all positions of the given player
    pub(crate) fn positions(&self, player: Disk) -> impl Iterator<Item=Position> {
        self.grid.into_iter()
            .flatten()
            .enumerate()
            .filter(move |(_, disk)| disk.is_some() && disk.unwrap() == player)
            .map(|(i, _)| Position::new(i / BOARD_SIZE, i % BOARD_SIZE))
    }
    
    /// Flips the disk at the given position
    /// Pre-conditions:
    /// * The given position must be occupied by a disk
    pub fn flip_at(&mut self, pos: &Position) -> Result<(), BoardError> {
        match self.disk_at(pos) {
            None => Err(InvalidArgument(format!("Board is empty at {}", pos))),
            Some(disk) => { 
                self.grid[pos.row][pos.col] = Some(if disk == Dark {Light} else {Dark});
                Ok(())
            }
        }
    }
    
    /// Returns all empty positions
    fn empty_positions(&self) -> impl Iterator<Item=Position> {
        self.grid.into_iter()
            .flatten()
            .enumerate()
            .filter(|(_, disk)| disk.to_owned().is_none())
            .map(|(i, _)| Position::new(i / BOARD_SIZE, i % BOARD_SIZE))
    }
    
    /// Returns the neighbours of the given position
    pub(crate) fn neighbours(&self, pos: &Position) -> impl Iterator<Item=Position> {
        let mut neighbours = Vec::with_capacity(9);
        
        let range: [i32; 3] = [-1, 0, 1];
        for i in range {
            for j in range {
                neighbours.push(Position::new((i + pos.row as i32) as usize,
                                              (j + pos.col as i32) as usize));
            }
        }
        
        let pos = pos.clone();
        neighbours.into_iter()
            .filter(move |neighbour| *neighbour != pos)
            .filter(|p| p.is_inbound())
    }
    
    /// Returns the neighbour from the given position at the given direction
    pub fn neighbour(&self, pos: &Position, dir: Direction) -> Option<Position> {
        let offset = match dir {
            North => (-1, 0),
            NorthEast => (-1, 1),
            East => (0, 1),
            SouthEast => (1, 1),
            South => (1, 0),
            SouthWest => (1, -1),
            West => (0, -1),
            NorthWest => (-1, -1),
        };
        
        let neighbour = Position::new((pos.row as i32 + offset.0) as usize, 
                      (pos.col as i32 + offset.1) as usize);
        
        if neighbour.is_inbound() {Some(neighbour)} else {None}
    }
    
    /// Clears this board
    pub fn clear(&mut self) {
        self.grid = [[None; BOARD_SIZE]; BOARD_SIZE];
    }
}

#[cfg(test)]
mod tests {
    use crate::board::{Board, BOARD_SIZE, Direction, Disk, Position};
    use crate::board::Direction::{East, North, NorthEast, NorthWest, South, SouthEast, SouthWest, West};
    use crate::board::Disk::{Dark, Light};

    #[test]
    fn new() {
        let board = Board::new();

        // This test is only correct when BOARD_SIZE == 8
        assert_eq!(BOARD_SIZE, 8);

        assert_eq!(board.grid[3][3], Some(Dark));
        assert_eq!(board.grid[4][4], Some(Dark));
        assert_eq!(board.grid[3][4], Some(Light));
        assert_eq!(board.grid[4][3], Some(Light));
        
        assert_eq!(board.to_string(),
        "\
        EEEEEEEE\n\
        EEEEEEEE\n\
        EEEEEEEE\n\
        EEEDLEEE\n\
        EEELDEEE\n\
        EEEEEEEE\n\
        EEEEEEEE\n\
        EEEEEEEE\n\
        "
        )
    }
    
    #[test]
    fn disk_at() { 
        let mut board = Board::new();
        
        let pos = Position::new(0, 0);
        assert!(board.disk_at(&pos).is_none());
        
        board.grid[pos.row][pos.col] = Some(Dark);
        assert_eq!(board.disk_at(&pos), Some(Dark));
    }
    
    #[test]
    fn flip_at() {
        let mut board = Board::new();

        let pos = Position::new(0, 0);
        assert!(board.flip_at(&pos).is_err());

        board.grid[0][0] = Some(Dark);
        assert!(board.flip_at(&pos).is_ok());
        assert_eq!(board.disk_at(&pos), Some(Light));

        assert!(board.flip_at(&pos).is_ok());
        assert_eq!(board.disk_at(&pos), Some(Dark));
    }
    
    #[test]
    fn empty_positions() {
        let mut board = Board::new();
        
        for i in 0..BOARD_SIZE {
            for j in 0..BOARD_SIZE {
                board.grid[i][j] = Some(Dark);
            }
        }
        
        assert_eq!(board.empty_positions().count(), 0);
        
        board.grid[0][0] = None;
        board.grid[0][7] = None;
        board.grid[3][6] = None;
        board.grid[6][3] = None;
        board.grid[7][0] = None;
        board.grid[7][7] = None;

        assert_eq!(board.empty_positions()
                       .map(|pos| pos.to_string())
                       .collect::<Vec<String>>(),
                   vec!["A1", "H1", "G4", "D7", "A8", "H8"]);
    }
    
    #[test]
    fn neighbours() {
        let board = Board::new();

        let get_result = |pos: &Position| -> Vec<String> {
            board.neighbours(&pos)
                .map(|pos| pos.to_string())
                .collect()
        };
        
        let pos = Position::new(0, 0);
        assert_eq!(get_result(&pos), vec!["B1", "A2", "B2"]);
        
        let pos = Position::new(0, BOARD_SIZE - 1);
        assert_eq!(get_result(&pos), vec!["G1", "G2", "H2"]);
        
        let pos = Position::new(BOARD_SIZE - 1, 0);
        assert_eq!(get_result(&pos), vec!["A7", "B7", "B8"]);

        let pos = Position::new(BOARD_SIZE - 1, BOARD_SIZE - 1);
        assert_eq!(get_result(&pos), vec!["G7", "H7", "G8"]);

        
        let pos = Position::new(0, 3);
        assert_eq!(get_result(&pos), vec!["C1", "E1", "C2", "D2", "E2"]);

        let pos = Position::new(3, BOARD_SIZE - 1);
        assert_eq!(get_result(&pos), vec!["G3", "H3", "G4", "G5", "H5"]);

        let pos = Position::new(BOARD_SIZE - 1, 3);
        assert_eq!(get_result(&pos), vec!["C7", "D7", "E7", "C8", "E8"]);

        let pos = Position::new(3, 0);
        assert_eq!(get_result(&pos), vec!["A3", "B3", "B4", "A5", "B5"]);

        
        let pos = Position::new(3, 3);
        assert_eq!(get_result(&pos), vec!["C3", "D3", "E3", "C4", "E4", "C5", "D5", "E5"]);
    }
    
    #[test]
    fn neighbour() {
        let board = Board::new();
        
        let get_result = |pos: &Position, dir: Direction| -> Option<String> {
            board.neighbour(pos, dir)
                .map(|neighbour| neighbour.to_string())
        };
        
        let pos = Position::new(3, 3);
        assert_eq!(get_result(&pos, North), Some("D3".to_string()));
        assert_eq!(get_result(&pos, NorthEast), Some("E3".to_string()));
        assert_eq!(get_result(&pos, East), Some("E4".to_string()));
        assert_eq!(get_result(&pos, SouthEast), Some("E5".to_string()));
        assert_eq!(get_result(&pos, South), Some("D5".to_string()));
        assert_eq!(get_result(&pos, SouthWest), Some("C5".to_string()));
        assert_eq!(get_result(&pos, West), Some("C4".to_string()));
        assert_eq!(get_result(&pos, NorthWest), Some("C3".to_string()));

        let pos = Position::new(0, 0);
        assert_eq!(get_result(&pos, North), None);
        assert_eq!(get_result(&pos, NorthEast), None);
        assert_eq!(get_result(&pos, SouthWest), None);
        assert_eq!(get_result(&pos, West), None);
        assert_eq!(get_result(&pos, NorthWest), None);
        
        let pos = Position::new(BOARD_SIZE - 1, BOARD_SIZE - 1);
        assert_eq!(get_result(&pos, NorthEast), None);
        assert_eq!(get_result(&pos, East), None);
        assert_eq!(get_result(&pos, SouthEast), None);
        assert_eq!(get_result(&pos, South), None);
        assert_eq!(get_result(&pos, SouthWest), None);
    }
    
    #[test]
    fn positions() {
        let mut board = Board::new();
        board.clear();

        board.grid[0][0] = Some(Dark);
        board.grid[1][1] = Some(Dark);
        board.grid[2][2] = Some(Light);

        let get_result = |player: Disk| -> Vec<String> {
            board.positions(player)
                .map(|pos| pos.to_string())
                .collect::<Vec<String>>()
        };
        
        assert_eq!(get_result(Dark), vec!["A1", "B2"]);
    }
    
    #[test]
    fn direction() {
        assert!(false);
    }
}
