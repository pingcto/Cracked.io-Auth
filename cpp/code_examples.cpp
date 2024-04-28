#include <iostream>
#include <fstream>
#include <string>
#include <curl/curl.h>
#include <iphlpapi.h>
#include <rpc.h>
#include <vector>
#include <cstdio> // For remove()
#pragma comment(lib, "rpcrt4.lib")
#pragma comment(lib, "iphlpapi.lib")

const std::vector<std::string> allowedGroups = { "12", "93" };

size_t WriteCallback(void* contents, size_t size, size_t nmemb, std::string* buffer) {
    size_t totalSize = size * nmemb;
    buffer->append((char*)contents, totalSize);
    return totalSize;
}

std::string get_hwid() {
    IP_ADAPTER_ADDRESSES* adapterAddresses = nullptr;
    ULONG bufferSize = 0;

    DWORD result = GetAdaptersAddresses(AF_UNSPEC, GAA_FLAG_INCLUDE_PREFIX, nullptr, adapterAddresses, &bufferSize);
    if (result == ERROR_BUFFER_OVERFLOW) {
        adapterAddresses = (IP_ADAPTER_ADDRESSES*)malloc(bufferSize);
        result = GetAdaptersAddresses(AF_UNSPEC, GAA_FLAG_INCLUDE_PREFIX, nullptr, adapterAddresses, &bufferSize);
    }

    std::string hwid;
    if (result == ERROR_SUCCESS) {
        PIP_ADAPTER_ADDRESSES adapter = adapterAddresses;
        while (adapter) {
            if (adapter->PhysicalAddressLength > 0) {
                for (DWORD i = 0; i < adapter->PhysicalAddressLength; i++) {
                    char hex[3];
                    sprintf_s(hex, "%02X", adapter->PhysicalAddress[i]);
                    hwid += hex;
                }
                break;
            }
            adapter = adapter->Next;
        }
    }

    free(adapterAddresses);
    return hwid;
}

bool save_auth_key(const std::string& key) {
    std::ofstream file("key.cio");
    if (!file) {
        std::cerr << "Unable to open file for writing." << std::endl;
        return false;
    }
    file << key;
    file.close();
    return true;
}

std::string read_auth_key() {
    std::ifstream file("key.cio");
    std::string key;
    if (file) {
        file >> key;
        file.close();
    }
    return key;
}

bool authenticateUser() {
    std::string authKey = read_auth_key();
    if (authKey.empty()) {
        std::cout << "Enter your auth key: ";
        std::getline(std::cin, authKey);
        if (!authKey.empty()) {
            save_auth_key(authKey);
        }
    }

    CURL* curl = curl_easy_init();
    if (!curl) {
        std::cerr << "Failed to initialize libcurl." << std::endl;
        return false;
    }

    std::string hwid = get_hwid();
    std::string response;

    curl_slist* headers = nullptr;
    headers = curl_slist_append(headers, "Content-Type: application/x-www-form-urlencoded");
    std::string postFields = "a=auth&k=" + authKey + "&hwid=" + hwid;

    curl_easy_setopt(curl, CURLOPT_URL, "https://cracked.to/auth.php");
    curl_easy_setopt(curl, CURLOPT_HTTPHEADER, headers);
    curl_easy_setopt(curl, CURLOPT_POSTFIELDS, postFields.c_str());
    curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteCallback);
    curl_easy_setopt(curl, CURLOPT_WRITEDATA, &response);

    CURLcode res = curl_easy_perform(curl);
    curl_slist_free_all(headers);
    curl_easy_cleanup(curl);

    if (res != CURLE_OK) {
        std::cerr << "curl_easy_perform() failed: " << curl_easy_strerror(res) << std::endl;
        return false;
    }

    // Parse response
    if (response.find("invalid key") != std::string::npos) {
        std::cout << "Key is invalid" << std::endl;
        if (!authKey.empty()) {
            remove("key.cio");
        }
        return false;
    }

    size_t groupPos = response.find("\"group\":\"");
    if (groupPos != std::string::npos) {
        size_t groupStart = groupPos + 9; // Length of "\"group\":\""
        size_t groupEnd = response.find("\"", groupStart);
        std::string group = response.substr(groupStart, groupEnd - groupStart);
        if (std::find(allowedGroups.begin(), allowedGroups.end(), group) != allowedGroups.end()) {
            size_t usernamePos = response.find("\"username\":\"");
            if (usernamePos != std::string::npos) {
                size_t usernameStart = usernamePos + 12; // Length of "\"username\":\""
                size_t usernameEnd = response.find("\"", usernameStart);
                std::string username = response.substr(usernameStart, usernameEnd - usernameStart);
                std::cout << "Welcome " << username << std::endl;
                // Save key to file if authed
                save_auth_key(authKey);
            }
        }
        else {
            std::cout << "You are not allowed to use this program" << std::endl;
        }
    }

    return true;
}

std::string ctoauth() {
    if (!authenticateUser()) {
        return "";
    }

    CURL* curl = curl_easy_init();
    if (!curl) {
        std::cerr << "Failed to initialize libcurl." << std::endl;
        return "";
    }

    std::string authkey = read_auth_key();
    std::string hwid = get_hwid();
    std::string response;

    curl_slist* headers = nullptr;
    headers = curl_slist_append(headers, "Content-Type: application/x-www-form-urlencoded");
    std::string postFields = "a=auth&k=" + authkey + "&hwid=" + hwid;

    curl_easy_setopt(curl, CURLOPT_URL, "https://cracked.to/auth.php");
    curl_easy_setopt(curl, CURLOPT_HTTPHEADER, headers);
    curl_easy_setopt(curl, CURLOPT_POSTFIELDS, postFields.c_str());
    curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteCallback);
    curl_easy_setopt(curl, CURLOPT_WRITEDATA, &response);

    CURLcode res = curl_easy_perform(curl);
    curl_slist_free_all(headers);
    curl_easy_cleanup(curl);

    if (res != CURLE_OK) {
        std::cerr << "curl_easy_perform() failed: " << curl_easy_strerror(res) << std::endl;
        return "";
    }

    return response;
}

int main() {
    std::string checkauthResponse = ctoauth();
    // Parse JSON response and perform necessary actions
    return 0;
}
