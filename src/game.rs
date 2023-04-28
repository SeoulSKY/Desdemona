use crate::board::{Board, Direction, Disk, Position};
use crate::board::Disk::Dark;

pub struct Action {
    player: Disk,
    placement: Position,
}

pub struct Game {
    board: Board,
    current_player: Disk,
}

impl Game {
    
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
    
    /// Perform the given action
    pub fn perform(&mut self, action: &Action) {
        self.board.place(action.player, &action.placement).unwrap();
        
        Direction::all();
        
        self.current_player = action.player.opponent();
    }
}

#[cfg(test)]
mod tests {
    use crate::game::Game;

    #[test]
    fn actions() {
        let game = Game::new();
        
        let get_result = || -> Vec<String> {
            game.actions()
                .map(|action| action.placement.to_string())
                .collect()
        };
        
        let mut result = get_result();
        let mut expected = vec!["E3", "F4", "C5", "D6"];
        
        result.sort();
        expected.sort();
        
        assert_eq!(result, expected);
    }
}
