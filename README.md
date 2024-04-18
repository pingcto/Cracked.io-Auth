# Authentication System Documentation

This document provides a comprehensive overview of the authentication system designed for integration with the Cracked.to platform. The system is implemented in both Python and C#, catering to different application environments. It ensures that only users with a valid authentication key and belonging to specific user groups (Premium+ and above) can access the application.

## Overview

The authentication system works by validating a user's authentication key and hardware ID (HWID) against the Cracked.to's authentication endpoint. It supports both initial authentication and subsequent verifications using a locally stored key. The system also distinguishes between regular users and those with upgraded memberships, allowing access only to those in the specified premium groups.

## Key Components

- **Authentication Key:** A unique key provided by the user, used for validating their access.
- **Hardware ID (HWID):** A unique identifier for the user's hardware, used to bind the authentication to a specific device.
- **Local Key Storage:** A file (key.dat) used to store the authentication key locally for subsequent access checks.

## Implementation

### Python Implementation

#### Dependencies

- `requests`: For making HTTP requests to the authentication endpoint.
- `json`: For parsing the response from the authentication endpoint.
- `uuid`: For generating the hardware ID.
- `os`: For file operations related to the local storage of the authentication key.

#### Authentication Flow

1. **Check for Existing Key:** The script first checks if a key.dat file exists locally. If it does, it reads the stored authentication key.
2. **Request Authentication:** It sends a POST request to `https://cracked.to/auth.php` with the action (a), key (k), and HWID.
3. **Validate Response:** The response is checked for a successful authentication flag. If successful, it further checks if the user belongs to the allowed premium groups.
4. **Handle New Users:** If no local key is found, it prompts the user to enter their authentication key, which is then validated as above.

### C# Implementation

#### Dependencies

- System libraries for file I/O, web requests, and JSON parsing.

#### Authentication Flow

Similar to the Python implementation, with the following specifics:
- **HWID Generation:** Uses the volume serial number of the system's primary drive as the HWID.
- **JSON Parsing:** Implements a custom JSON parser to handle the response from the authentication endpoint.

## Upgraded User Check

Both implementations include logic to verify if the authenticated user belongs to specific user groups considered as "Premium+" or higher. This is done by checking the user's group ID against a predefined list of allowed group IDs.

### Code Examples

#### Python Example

```python
import requests
import json
import uuid
import os

# List of premium group IDs
premiumplus = ['11', '12', '93', '96', '97', '99', '100', '101', '4', '3', '6', '94', '92']

def ctoauth():
    # Check for existing key
    if os.path.isfile('key.dat'):
        current_hwid = uuid.getnode()
        with open('key.dat', 'r') as f:
            auth = f.read()
        data = {"a": "auth", "k": str(auth), "hwid": str(current_hwid)}
        checkauth = requests.post('https://cracked.to/auth.php', data=data)
        json1 = json.loads(checkauth.text)
        if '"auth":true' in checkauth.text and any(json1["group"] in s for s in premiumplus):
            print("Welcome: " + json1["username"])
        else:
            print("Access Denied.")
            exit()
    else:
        # Handle new user authentication
        authkey = input('Insert Cracked.to Auth Key: ')
        # Similar to above, send request with new key
```
#### C# Example
```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Net;
using System.Text;

namespace CrackedAuth {
    internal class Auth {
        public static void login() {
            string KEY = File.Exists("key.dat") ? File.ReadAllText("key.dat") : Console.ReadLine();
            string hwid = GetHWID();
            var data = Encoding.UTF8.GetBytes($"a=auth&k={KEY}&hwid={hwid}");
            // Send request and handle response similar to Python example
        }

        private static string GetHWID() {
            // Implementation to retrieve HWID
            return "HWID";
        }
    }
}
```
### Security Considerations

  Key Storage: The local storage of the authentication key (key.dat) should be secured to prevent unauthorized access.
  
  HWID Binding: The use of HWID as part of the authentication process helps in binding the authentication to a specific hardware, reducing the risk of key sharing.

### Conclusion
  
  This authentication system provides a secure method to ensure that only authorized and premium users can access the application. It leverages both local key storage and hardware-bound authentication to enhance security and user management.
