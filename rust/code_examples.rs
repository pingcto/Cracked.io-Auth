```rust
use reqwest::Client;
use serde::{Deserialize, Serialize};
use std::{fs, io::{self, Write}, path::Path};

#[derive(Serialize, Deserialize)]
struct AuthResponse {
    auth: bool,
    username: Option<String>,
}

async fn read_or_create_key() -> io::Result<String> {
    let key_path = Path::new("key.dat");
    match fs::read_to_string(key_path) {
        Ok(key) => Ok(key),
        Err(_) => {
            print!("Enter your auth key: ");
            io::stdout().flush()?;
            let mut key = String::new();
            io::stdin().read_line(&mut key)?;
            fs::write(key_path, &key)?;
            Ok(key.trim().to_string())
        }
    }
}

async fn get_hwid() -> String {
    "SIMULATED_HWID".to_string() // Replace with actual HWID retrieval logic.
}

async fn authenticate(key: String) -> Result<(), Box<dyn std::error::Error>> {
    let client = Client::new();
    let hwid = get_hwid().await;
    let res = client.post("https://cracked.to/auth.php")
        .form(&[("a", "auth"), ("k", &key), ("hwid", &hwid)])
        .send()
        .await?;

    let response: AuthResponse = res.json().await?;
    if response.auth {
        println!("Welcome, {}", response.username.unwrap_or_default());
    } else {
        println!("Access denied.");
    }

    Ok(())
}

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    let key = read_or_create_key().await?;
    authenticate(key).await
}
```
