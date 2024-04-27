import requests
import json
import uuid
import os

# Configuration
PREMIUM_GROUP_IDS = {'11', '12', '93', '96', '97', '99', '100', '101', '4', '3', '6', '94', '92'}
AUTH_URL = 'https://cracked.to/auth.php'

def get_hwid():
    """Generate and return the hardware ID of the current device."""
    return str(uuid.getnode())

def read_auth_key():
    """Attempt to read the auth key from 'key.dat', returning None if not found."""
    try:
        with open('key.dat', 'r') as file:
            return file.read().strip()
    except FileNotFoundError:
        return None

def save_auth_key(auth_key):
    """Save the provided auth key to 'key.dat'."""
    with open('key.dat', 'w') as file:
        file.write(auth_key)
    print("Auth key saved successfully.")

def authenticate_user(auth_key):
    """Authenticate the user with Cracked.to, returning the response JSON or None on error."""
    data = {"a": "auth", "k": auth_key, "hwid": get_hwid()}
    try:
        response = requests.post(AUTH_URL, data=data)
        response.raise_for_status()
        return response.json()
    except requests.exceptions.RequestException as e:
        print(f"Authentication error: {e}")
        return None

def is_user_premium(user_groups):
    """Determine if any of the user's groups are in the premium group list."""
    return any(group in PREMIUM_GROUP_IDS for group in user_groups)

def main():
    auth_key = read_auth_key()
    if not auth_key:
        auth_key = input('Please insert your Cracked.to Auth Key: ').strip()
        save_auth_key(auth_key)

    response = authenticate_user(auth_key)
    if response and response.get("auth"):
        user_groups = response.get("group", "").split(',')
        if is_user_premium(user_groups):
            print(f"Welcome, {response.get('username')}! Authentication successful.")
        else:
            print("Access denied. You need to be in a Premium+ group.")
    else:
        print("Authentication failed. Please check your credentials.")

if __name__ == "__main__":
    main()
