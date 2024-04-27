package main

import (
    "bufio"
    "encoding/json"
    "fmt"
    "net/http"
    "os"
    "strings"
)

type AuthResponse struct {
    Auth     bool   `json:"auth"`
    Username string `json:"username"`
}

func readOrCreateKey() (string, error) {
    keyPath := "key.dat"
    var key string
    if file, err := os.Open(keyPath); err == nil {
        defer file.Close()
        scanner := bufio.NewScanner(file)
        if scanner.Scan() {
            key = scanner.Text()
        }
    } else {
        fmt.Print("Enter your auth key: ")
        scanner := bufio.NewScanner(os.Stdin)
        if scanner.Scan() {
            key = scanner.Text()
            os.WriteFile(keyPath, []byte(key), 0644)
        }
    }
    return key, nil
}

func getHWID() string {
    return "SIMULATED_HWID" // Replace with actual HWID retrieval logic.
}

func authenticate(key string) {
    hwid := getHWID()
    resp, err := http.PostForm("https://cracked.to/auth.php",
        url.Values{"a": {"auth"}, "k": {key}, "hwid": {hwid}})
    if err != nil {
        fmt.Println("Failed to authenticate:", err)
        return
    }
    defer resp.Body.Close()

    var response AuthResponse
    json.NewDecoder(resp.Body).Decode(&response)
    if response.Auth {
        fmt.Println("Welcome, " + response.Username)
    } else {
        fmt.Println("Access denied.")
    }
}

func main() {
    key, err := readOrCreateKey()
    if err != nil {
        fmt.Println("Error:", err)
        return
    }
    authenticate(key)
}
