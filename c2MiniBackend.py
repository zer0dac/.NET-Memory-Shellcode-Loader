from Crypto.Cipher import AES
from Crypto.Util.Padding import pad
from base64 import b64encode
from flask import Flask, jsonify
import base64


app = Flask(__name__)
def read_hex_file(file_path):
with open(file_path, "r") as file:
hex_data = file.read().strip() 
hex_data = hex_data.replace("0x", "").replace(",", "")
return bytes.fromhex(hex_data) 
def encrypt_hex_file(file_path):
key = bytes.fromhex("1f768bd57cbf021b251deb0791d8c197") 
iv = bytes.fromhex("ee7d63936ac1f286d8e4c5ca82dfa5e2") 
cipher = AES.new(key, AES.MODE_CBC, iv)
file_data = read_hex_file(file_path)
encrypted_data = cipher.encrypt(pad(file_data, AES.block_size))
encrypted_base64 = base64.b64encode(encrypted_data).decode('utf-8')
print(f"Şifrelenmiş Veri (Base64): {encrypted_base64}")
return encrypted_base64
@app.route('/get-base64-file', methods=['GET'])
def serve_encrypted_file():
file_path = '/home/parrot/my-server/test.txt'
base64_encrypted = encrypt_hex_file(file_path)
return jsonify({'file': base64_encrypted})
if __name__ == '__main__':
app.run(host='0.0.0.0', port=5000)