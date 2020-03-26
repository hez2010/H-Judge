use serde::{Deserialize, Serialize};
#[derive(Serialize, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct ExecOptions {
    pub exec: String,
    pub args: String,
    pub working_dir: String,
    pub std_err_redirect_file: String,
    pub input_file: String,
    pub output_file: String,
    pub time_limit: i64,
    pub memory_limit: i64,
    pub is_std_io: bool,
    pub active_process_limit: u32,
}
