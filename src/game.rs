use crate::board::{Board, Direction, Disk, Position};
use crate::board::Disk::Dark;

pub struct Action {
    player: Disk,
    placement: Position,
}

#[derive(Clone)]
pub struct Game {
    board: Board,
    current_player: Disk,
}

impl Game {
    
    /// Creates a new state of the game
    pub fn new() -> Self {
        Self {
            board: Board::new(),
            current_player: Dark,
        }
    }
    
    /// Returns the possible actions of the current player
    pub fn actions(&self) -> impl Iterator<Item=Action> + '_ {
        self.board.positions(self.current_player)
            .flat_map(|pos| {
                self.board.neighbours(&pos)
                    .map(move |neighbour| (pos.direction(&neighbour), neighbour))
            })
            .filter(|(_, neighbour)| self.board.disk_at(neighbour) == 
                Some(self.current_player.opponent()))
            .flat_map(|(dir, neighbour)| self.board.neighbour(&neighbour, dir))
            .filter(|pos| self.board.disk_at(pos).is_none())
            .map(|pos| Action{player: self.current_player, placement: pos})
    }
    
    /// Returns the new state with the action applied
    pub fn result(&self, action: &Action) -> Self {
        let mut game = self.clone();

        game.board.place(action.player, &action.placement).unwrap();
        
        for dir in Direction::all() {
            let neighbour = game.board.neighbour(&action.placement, dir);
            if neighbour.is_none() {
                continue;
            }
            
            let mut path = Vec::new();
            
            let mut walker = neighbour.unwrap();
            while game.board.disk_at(&walker) == Some(action.player.opponent()) {
                path.push(walker.clone());

                let neighbour = game.board.neighbour(&walker, dir);
                if neighbour.is_none() {
                    break;
                }
                walker = neighbour.unwrap();
            }
            
            if game.board.disk_at(&walker) == Some(action.player) {
                for pos in path {
                    game.board.flip_at(&pos).unwrap();
                }
            }
        }

        game.current_player = action.player.opponent();
        game
    }
}

#[cfg(test)]
mod tests {
    use itertools::{Itertools, sorted};
    use crate::board::BOARD_SIZE;
    use crate::board::Disk::{Dark, Light};
    use crate::board::Position;
    use crate::game::{Action, Game};

    #[test]
    fn actions() {
        let game = Game::new();
        
        let get_result = || -> Vec<String> {
            game.actions()
                .map(|action| action.placement.to_string())
                .collect()
        };
        
        assert_eq!(get_result().into_iter().sorted().collect_vec(),
                   vec!["E3", "F4", "C5", "D6"].into_iter()
                       .map(|s| s.to_string())
                       .sorted()
                       .collect_vec());
    }
    
    #[test]
    fn result() {
        let mut game = Game::new();

        for j in 1..BOARD_SIZE {
            game.board.place(Light, &Position::new(0, j)).unwrap()
        }
        game.board.flip_at(&Position::new(0, BOARD_SIZE - 1)).unwrap();
        
        let mut game = game.result(&Action{player: Dark, placement: Position::new(0, 0)});
        for j in 0..BOARD_SIZE {
            assert_eq!(game.board.disk_at(&Position::new(0, j)), Some(Dark))
        }
        
        // -------------------------

        game.board.clear();

        for i in 1..BOARD_SIZE {
            game.board.place(Light, &Position::new(i, i)).unwrap()
        }
        game.board.flip_at(&Position::new(BOARD_SIZE - 1, BOARD_SIZE - 1)).unwrap();

        let game = game.result(&Action{player: Dark, placement: Position::new(0, 0)});
        for i in 0..BOARD_SIZE {
            assert_eq!(game.board.disk_at(&Position::new(i, i)), Some(Dark))
        }
    }
}
