#include <iostream>
#include <cpr/cpr.h>
#include <nlohmann/json.hpp>
#include <filesystem>
#include <fstream>

std::string read_or_create_key() {
    std::string key_path = "key.dat";
    std::string key;
    std::ifstream key_file(key_path);
    if (key_file) {
        std::getline(key_file, key);
    } else {
        std::cout << "Enter your auth key: ";
        std::cin >> key;
        std::ofstream out_key_file(key_path);
        out_key_file << key;
    }
    return key;
}

std::string get_hwid() {
    // Simulating hardware ID retrieval; replace with actual method to obtain HWID.
    return "SIMULATED_HWID";
}

void authenticate(const std::string& key) {
    std::string hwid = get_hwid();
    cpr::Response r = cpr::Post(cpr::Url{"https://cracked.to/auth.php"},
                                cpr::Payload{{"a", "auth"}, {"k", key}, {"hwid", hwid}});
    if (r.status_code == 200) {
        nlohmann::json response = nlohmann::json::parse(r.text);
        if (response.contains("auth") && response["auth"] == true) {
            std::cout << "Welcome, " << response["username"].get<std::string>() << std::endl;
        } else {
            std::cout << "Access denied." << std::endl;
        }
    } else {
        std::cout << "Failed to authenticate." << std::endl;
    }
}

int main() {
    std::string key = read_or_create_key();
    authenticate(key);
    return 0;
}
