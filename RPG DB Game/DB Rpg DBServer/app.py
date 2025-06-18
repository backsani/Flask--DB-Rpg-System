# app.py
from flask import Flask, request, jsonify
from flask_cors import CORS
from werkzeug.security import generate_password_hash, check_password_hash
import pymysql
import jwt
import datetime

SECRET_KEY = "VZK4r8TNgA2RQv1l7mOwIm3eZb6KfhuFZqxR4ZmwWQ8"

app = Flask(__name__)
CORS(app)

# DB 연결 함수
def get_db():
    return pymysql.connect(
        host="127.0.0.1",
        user="rpg",
        password="rpg",
        db="rpg",
        charset="utf8",
        cursorclass=pymysql.cursors.DictCursor
    )

def get_user_from_token():
    auth_header = request.headers.get("Authorization")
    if not auth_header or not auth_header.startswith("Bearer "):
        return None
    
    token = auth_header.split()[1]
    
    try:
        payload = jwt.decode(token, SECRET_KEY, algorithms=["HS256"])
        return payload["user_id"]
    except jwt.ExpiredSignatureError:
        return None
    except jwt.InvalidTokenError:
        return None

# 회원가입
@app.route("/signup", methods=["POST"])
def signup():
    data = request.get_json()
    email = data.get("email")
    password = data.get("password")
    name = data.get("name")

    password_hash = generate_password_hash(password)

    conn = get_db()
    try:
        with conn.cursor() as cur:
            cur.execute("SELECT * FROM users WHERE email=%s", (email,))
            if cur.fetchone():
                return jsonify({"success": False, "message": "Email already exists"})
            cur.execute("INSERT INTO users (email, user_password, user_name) VALUES (%s, %s, %s)", (email, password_hash, name))
            conn.commit()
    finally:
        conn.close()

    return jsonify({"success": True})

# 로그인
@app.route("/login", methods=["POST"])
def login():
    data = request.get_json()
    email = data.get("email")
    password = data.get("password")

    conn = get_db()
    try:
        with conn.cursor() as cur:
            cur.execute("SELECT * FROM users WHERE email=%s", (email,))
            user = cur.fetchone()
            if user and check_password_hash(user["user_password"], password):
                payload = { "user_id" : user["id"], "exp" : datetime.datetime.utcnow() + datetime.timedelta(hours=1) }
                token = jwt.encode(payload, SECRET_KEY, algorithm="HS256")
                if isinstance(token, bytes):
                    token = token.decode("utf-8")
                
                return jsonify({"success": True, "token": token, "user_name": user["user_name"], "user_id": user["id"]})
            else:
                return jsonify({"success": False, "message": "Invalid credentials"})
    finally:
        conn.close()
        
# 캐릭터 생성
@app.route("/character/create", methods=["POST"])
def characterCreate():
    data = request.get_json()
    name = data.get("name")
    characterClass = data.get("characterClass")
    gender = data.get("gender")
    user_id = get_user_from_token()
    
    if not user_id:
        return jsonify({"success": False, "message": "Invalid token"})
    if not name or characterClass is None or gender is None:
        return jsonify({"success": False, "message": "Missing fields"})

    
    conn = get_db()
    try:
        with conn.cursor() as cur:
            cur.execute("SELECT * FROM characters WHERE character_name=%s", (name,))
            if cur.fetchone():
                return jsonify({"success": False, "message": "character_name already exists"})
            
            cur.execute("INSERT INTO characters (user_id, character_name, character_class, gender, character_level, xp) VALUES (%s, %s, %s, %s, %s, %s)", (user_id, name, characterClass, gender, 1, 0))
            character_id = cur.lastrowid
            
            cur.execute("INSERT INTO character_stat (character_id, hp, atk, matk, def, speed) VALUES (%s, %s, %s, %s, %s, %s)", (character_id, 0, 0, 0, 0, 0))
            
            slots = [(character_id, i, 0) for i in range(1, 9)]
            cur.executemany("INSERT INTO inventory (character_id, slot_index, item_code) VALUES (%s, %s, %s)", slots)
            
            conn.commit()
            
    finally:
        conn.close()
        
    return jsonify({"success": True})

# 캐릭터 리스트 조회
@app.route("/character/list", methods=["GET"])
def characterList():
    user_id = get_user_from_token()
    if not user_id:
        return jsonify({"success": False, "message": "Invalid token"})

    conn = get_db()
    try:
        with conn.cursor() as cur:
            cur.execute("SELECT id, character_name, character_class, gender, character_level, xp FROM characters WHERE user_id = %s", (user_id,))
            characters = cur.fetchall()
            
            cur.execute("SELECT last_character_id, last_world_id FROM users WHERE id = %s", (user_id,))
            last_info = cur.fetchone()
            
    finally:
        conn.close()

    return jsonify({"success": True, "characters": characters, "last_info": last_info})

# 캐릭터 스텟 조회
@app.route("/character/stat", methods=["POST"])
def characterStat():
    user_id = get_user_from_token()
    
    data = request.get_json()
    character_id = data.get("character_id")
    
    if not user_id:
        return jsonify({"success": False, "message": "Invalid token"})
    
    conn = get_db()
    try:
        with conn.cursor() as cur:
            cur.execute("SELECT * FROM characters WHERE id = %s AND user_id = %s", (character_id, user_id))
            character = cur.fetchone()

            if not character:
                return jsonify({"success": False, "message": "Character not found or unauthorized"})

            cur.execute("SELECT c.character_name, cs.hp, cs.atk, cs.matk, cs.def, cs.speed FROM characters c JOIN character_stat cs ON c.id = cs.character_id WHERE c.id = %s", (character_id, ))

            character_stat = cur.fetchone()
            if character_stat:
                return jsonify({"success": True, "character_stat": character_stat})
            else:
                return jsonify({"success": False, "message": "Stat not found"})
    finally:
        conn.close()

# 캐릭터 인벤토리 조회
@app.route("/inventory/list", methods=["POST"])
def inventoryGet():
    user_id = get_user_from_token()
    if not user_id:
        return jsonify({"success": False, "message": "Invalid token"})
    
    data = request.get_json()
    character_id = data.get("character_id")
    conn = get_db()
    try:
        with conn.cursor() as cur:
            cur.execute("SELECT slot_index, item_code FROM inventory WHERE character_id = %s", (character_id,))
            inventory = cur.fetchall()
    finally:
        conn.close()
    
    return jsonify({"success" : True, "inventory" : inventory})

# 월드 리스트 조회
@app.route("/map/list", methods=["GET"])
def worldList():
    conn = get_db()
    try:
        with conn.cursor() as cur:
            cur.execute("SELECT id, world_name, world_explan, open_level FROM world")
            world = cur.fetchall()
    finally:
        conn.close()
    
    return jsonify({"success" : True, "worlds": world})

# 최신 정보 저장
@app.route("/user/update_last_info", methods=["POST"])
def update_last_info():
    user_id = get_user_from_token()
    if not user_id:
        return jsonify({"success": False, "message": "Invalid token"})

    data = request.get_json()
    last_character_id = data.get("last_character_id")
    last_world_id = data.get("last_world_id")

    conn = get_db()
    try:
        with conn.cursor() as cur:
            cur.execute(
                "UPDATE users SET last_character_id=%s, last_world_id=%s WHERE id=%s",
                (last_character_id, last_world_id, user_id)
            )
            conn.commit()
    finally:
        conn.close()

    return jsonify({"success": True})

# NPC 리스트 조회
@app.route("/npc/list", methods=["POST"])
def npcList():
    data = request.get_json()
    world_id = data.get("worldId")
    user_id = get_user_from_token()
    
    if not user_id:
        return jsonify({"success": False, "message": "Invalid token"})
    
    conn = get_db()
    try:
        with conn.cursor() as cur:
            cur.execute("SELECT id, pos_x, pos_y, pos_z, npc_name, reward FROM npc WHERE world_id = %s", (world_id, ))
            npc = cur.fetchall()
    
    finally:
        conn.close()
    
    return jsonify({"success" : True, "npcStat": npc})
    
# 캐릭터 저장
@app.route("/character/save", methods=["POST"])
def saveData():
    user_id = get_user_from_token()
    if not user_id:
        return jsonify({"success": False, "message": "Invalid token"})
    
    data = request.get_json()
    character_id = data.get("character_id")
    xp = data.get("xp")
    level = data.get("level")
    hp = data.get("hp")
    atk = data.get("atk")
    matk = data.get("matk")
    defence = data.get("def")
    speed = data.get("speed")
    
    conn = get_db()
    try:
        with conn.cursor() as cur:
            cur.execute("UPDATE characters SET xp = %s, character_level = %s WHERE id= %s", (xp, level, character_id))
            cur.execute("UPDATE character_stat SET hp = %s, atk = %s, matk = %s, def = %s, speed = %s WHERE character_id= %s", (hp, atk, matk, defence, speed, character_id))
            conn.commit()
    finally:
        conn.close()
        
    return jsonify({"success": True})

if __name__ == "__main__":
    app.run(debug=True)