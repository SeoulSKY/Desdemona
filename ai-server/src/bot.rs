use std::cmp::{max, min};
use std::collections::HashMap;

use crate::errors::Error;
use crate::errors::Error::InvalidArgument;
use crate::game::{Action, Game};
use crate::game::Player;

pub struct Bot {
    depth_limit: u32,
    game: Game,
    minimax_cache: HashMap<Game, i32>,
}

impl Bot {
    
    /// Creates a new instance of the bot
    pub fn new(intelligence: u32) -> Self {
        Self {
            depth_limit: intelligence,
            game: Game::new(),
            minimax_cache: HashMap::new(),
        }
    }
    
    /// Decides the next action from the given state
    /// 
    /// Pre-conditions:
    /// * self.game.current_player() == Player::Bot
    pub fn decide(&mut self, game: Game) -> Result<(Action, Game), Error> {
        assert_eq!(self.game.current_player(), Player::Bot);
        
        let mut bot_best = i32::MIN;
        let human_best = i32::MAX;
        
        let mut minimax_value = bot_best;
        let mut best_action= Action::default();
        let mut best_result= Game::default();
        let mut decided = false;
        let mut num_actions = 0;
        
        for act in game.actions(Player::Bot) {
            num_actions += 1;
            let result = game.result(&act);
            let value = self.min_value(result.clone(), bot_best, human_best, 1);
            if value > minimax_value {
                minimax_value = value;
                best_action = act;
                best_result = result;
                decided = true;
            }
            bot_best = max(bot_best, minimax_value);
        }

        if num_actions == 0 {
            return Err(InvalidArgument(format!("No actions are available from the given game.")));
        }
        
        assert!(decided);
        
        Ok((best_action, best_result))
    }
    
    /// Finds the min value of the minimax
    fn min_value(&mut self, game: Game, max_best: i32, mut min_best: i32, depth: u32) -> i32 {
        if game.is_over() {
            return game.utility();
        } else if depth > self.depth_limit {
            return self.evaluate(game);
        }

        let mut min_best_here = i32::MAX;

        for act in game.actions(Player::Human) {
            let result = game.result(&act);
            let value = self.max_value(result, max_best, min_best, depth + 1);
            if value < min_best_here {
                min_best_here = value;
            }
            if min_best_here <= min_best {
                return min_best_here;
            }
            min_best = min(min_best, min_best_here);
        }

        return min_best_here;
    }
    
    /// Finds the max value of the minimax
    fn max_value(&mut self, game: Game, mut max_best: i32, min_best: i32, depth: u32) -> i32 {
        if game.is_over() {
            return game.utility();
        } else if depth > self.depth_limit {
            return self.evaluate(game);
        }
        
        let mut max_best_here = i32::MIN;
        
        for act in game.actions(Player::Bot) {
            let result = game.result(&act);
            let value = self.min_value(result, max_best, min_best, depth + 1);
            if value > max_best_here {
                max_best_here = value;
            }
            if max_best_here >= min_best {
                return max_best_here;
            }
            max_best = max(max_best, max_best_here);
        }
        
        return max_best_here;
    }
    
    /// Evaluates the given game to a value
    fn evaluate(&mut self, game: Game) -> i32 {
        if self.minimax_cache.contains_key(&game) {
            return self.minimax_cache.get(&game).copied().unwrap();
        }
        
        let value = game.evaluate();
        self.minimax_cache.insert(game, value);
        value
    }
}
