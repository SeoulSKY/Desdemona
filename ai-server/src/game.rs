use std::fmt::{Display, Formatter};

use crate::board::{Board, Direction, Disk, Position};
use crate::board::Disk::{Dark, Light};
use crate::errors::Error;
use crate::errors::Error::ParseError;
use crate::game::Player::{Bot, Human};

pub const BOT_CHAR: char = 'B';
pub const HUMAN_CHAR: char = 'H';

pub const MAX_BEST_EVALUATION: i32 = i32::MAX;
pub const MIN_BEST_EVALUATION: i32 = i32::MIN;

const PLACEMENT_WEIGHT: i32 = 1;
const MOBILITY_WEIGHT: i32 = 1;

#[derive(Default, Debug, Copy, Clone, PartialEq, Eq, Hash)]
pub enum Player {
    #[default]
    Bot,
    Human,
}

impl Player {
    
    /// Parses the given character to a player
    pub fn parse(ch: char) -> Result<Self, Error> {
        match ch {
            BOT_CHAR => Ok(Bot),
            HUMAN_CHAR => Ok(Human),
            _ => Err(ParseError(format!("Invalid character to parse into a player: {}", ch)))
        }
    }
    
    /// Returns the opponent of this player
    pub fn opponent(&self) -> Self {
        match *self {
            Bot => Human,
            Human => Bot,
        }
    }
    
    /// Returns the corresponding disk of this player
    pub fn disk(&self) -> Disk {
        match *self {
            Bot => Light,
            Human => Dark,
        } 
    }
}

impl Display for Player {
    fn fmt(&self, f: &mut Formatter<'_>) -> std::fmt::Result {
        write!(f, "{}", match *self {
            Bot => BOT_CHAR,
            Human => HUMAN_CHAR,
        })
    }
}


#[derive(Default, PartialEq)]
pub struct Action {
    player: Player,
    placement: Position,
}

impl Action {
    /// Parses the given player and placement into an Action
    pub fn parse(player: Player, placement: Position) -> Self {
        Self {
            player,
            placement
        }
    }
}

impl Display for Action {
    fn fmt(&self, f: &mut Formatter<'_>) -> std::fmt::Result {
        write!(f, "{}", self.placement)
    }
}

#[derive(Clone, PartialEq, Eq, Hash, Default)]
pub struct Game {
    board: Board,
    current_player: Player,
    winner: Option<Player>,
}

impl Game {
    
    /// Creates a new state of the game
    pub fn new() -> Self {
        Self {
            board: Board::new(),
            current_player: Bot,
            winner: None,
        }
    }
    
    /// Parses the given data into a Game
    pub fn parse(board: Board, current_player: Player) -> Self {
        let mut game = Self {
            board,
            current_player,
            winner: None,
        };
        
        if game.is_over() {
            game.set_winner();
        }
        
        game
    }
    
    /// Returns the current player of this turn
    pub fn current_player(&self) -> Player {
        self.current_player
    }
    

    /// Returns the possible actions of the given player
    pub fn actions(&self, player: Player) -> impl Iterator<Item=Action> + '_ {
        let mut actions = Vec::new();
        
        for position in self.board.positions(player.disk()) {
            for direction in Direction::all() {
                let mut distance = 1;
                let mut walker = self.board.neighbour(&position, direction);
                
                while walker.is_some() {
                    let disk =  self.board.disk(walker.as_ref().unwrap());
                    
                    if disk.is_none() {
                        if distance > 1 {
                            actions.push(Action { player, placement: walker.unwrap() });
                        }
                        break;
                    }

                    if disk.unwrap() != player.opponent().disk() {
                        break;
                    }

                    distance += 1;
                    walker = self.board.neighbour(&walker.unwrap(), direction);
                }
            }
        }
        
        actions.into_iter()
    }
    
    
    /// Returns the new state with the action applied
    pub fn result(&self, action: &Action) -> Self {
        let mut game = self.clone();

        game.board.place(action.player.disk(), &action.placement).unwrap();
        
        for dir in Direction::all() {
            let neighbour = game.board.neighbour(&action.placement, dir);
            if neighbour.is_none() {
                continue;
            }
            
            let mut path = Vec::new();
            
            let mut walker = neighbour.unwrap();
            while game.board.disk(&walker) == Some(action.player.opponent().disk()) {
                path.push(walker.clone());

                let neighbour = game.board.neighbour(&walker, dir);
                if neighbour.is_none() {
                    break;
                }
                walker = neighbour.unwrap();
            }
            
            if game.board.disk(&walker) == Some(action.player.disk()) {
                for pos in path {
                    game.board.flip(&pos).unwrap();
                }
            }
        }

        game.current_player = action.player.opponent();
        if game.is_over() {
            game.set_winner();
        }
        game
    }
    
    fn set_winner(&mut self) {
        assert!(self.is_over());

        let num_bot_disks = self.board.positions(Bot.disk()).count();
        let num_human_disks = self.board.positions(Human.disk()).count();

        self.winner = if num_bot_disks > num_human_disks {
            Some(Bot)
        } else if num_human_disks > num_bot_disks {
            Some(Human)
        } else {
            None
        };
    }
    
    /// Checks if this game is over
    pub fn is_over(&self) -> bool {
        self.actions(Bot).next() == None && self.actions(Human).next() == None
    }
    
    /// Returns the winner of the game
    /// 
    /// Pre-conditions
    /// * self.is_over()
    pub fn winner(&self) -> Option<Player> {
        assert!(self.is_over());
        self.winner
    }
    
    /// Returns the board of the game
    pub fn board(&self) -> &Board {
        &self.board
    }
    
    /// Returns the utility of this game
    /// 
    /// Pre-conditions:
    /// * self.is_over()
    pub fn utility(&self) -> i32 {
        assert!(self.is_over());
        
        match self.winner {
            Some(Bot) => MAX_BEST_EVALUATION,
            Some(_) => MIN_BEST_EVALUATION,
            None => 0,
        }
    }
    
    /// Evaluates this game state to a value
    /// Pre-conditions:
    /// * !self.is_over()
    pub fn evaluate(&self) -> i32 {
        assert!(!self.is_over());
        
        PLACEMENT_WEIGHT * (
            self.board.positions(Bot.disk())
                .map(|p| p.weight())
                .sum::<i32>() -
            self.board.positions(Human.disk())
                .map(|p| p.weight())
                .sum::<i32>()
        ) + MOBILITY_WEIGHT * (
            self.actions(Bot)
                .count() as i32 -
            self.actions(Human)
                .count() as i32
        )
    }
}

#[cfg(test)]
mod tests {
    use itertools::Itertools;

    use crate::board::{Board, BOARD_SIZE};
    use crate::board::Position;
    use crate::game::{Action, Game};
    use crate::game::Player::{Bot, Human};

    #[test]
    fn actions() {
        let game = Game::new();
        
        let get_result = |game: Game| -> Vec<String> {
            game.actions(Bot)
                .map(|action| action.placement.to_string())
                .collect()
        };
        
        assert_eq!(get_result(game).into_iter().sorted().collect_vec(),
                   vec!["2,3", "3,2", "4,5", "5,4"].into_iter()
                       .map(|s| s.to_string())
                       .sorted()
                       .collect_vec());
        
        let mut board = Board::new();
        board.clear();
        for i in 1..BOARD_SIZE-1 {
            board.place(Human.disk(), &Position::new(i, 0)).unwrap();
        }
        board.place(Bot.disk(), &Position::new(BOARD_SIZE-1, 0)).unwrap();
        
        let game = Game::parse(board, Bot);
        assert_eq!(get_result(game).into_iter().sorted().collect_vec(),
                   vec!["0,0"].into_iter()
                       .map(|s| s.to_string())
                       .sorted()
                       .collect_vec())
    }
    
    #[test]
    fn result() {
        let mut game = Game::new();

        for j in 1..BOARD_SIZE {
            game.board.place(Human.disk(), &Position::new(0, j)).unwrap()
        }
        game.board.flip(&Position::new(0, BOARD_SIZE - 1)).unwrap();
        
        let mut game = game.result(&Action{player: Bot, placement: Position::new(0, 0)});
        for j in 0..BOARD_SIZE {
            assert_eq!(game.board.disk(&Position::new(0, j)), Some(Bot.disk()))
        }
        
        // -------------------------

        game.board.clear();

        for i in 1..BOARD_SIZE {
            game.board.place(Human.disk(), &Position::new(i, i)).unwrap()
        }
        game.board.flip(&Position::new(BOARD_SIZE - 1, BOARD_SIZE - 1)).unwrap();

        let game = game.result(&Action{player: Bot, placement: Position::new(0, 0)});
        for i in 0..BOARD_SIZE {
            assert_eq!(game.board.disk(&Position::new(i, i)), Some(Bot.disk()))
        }
    }
}
