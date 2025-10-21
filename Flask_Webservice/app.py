from flask import Flask, request, jsonify
import psycopg2
from datetime import datetime
from flask_cors import CORS

app = Flask(__name__)
CORS(app)

# DB 연결 정보
DB_HOST = "your_Endpoint"
DB_NAME = "your_db_name"
DB_USER = "your_id"
DB_PASS = "your_password"

def get_db_connection():
    return psycopg2.connect(
        host=DB_HOST,
        database=DB_NAME,
        user=DB_USER,
        password=DB_PASS
    )

@app.route("/api/insert_home_prj_data", methods=["POST"])
def insert_home_prj_data():
    data = request.get_json()

    try:
        p_insert_datetime = data.get("insert_datetime")  # 문자열 형태 (예: "2025-10-18T12:00:00")
        p_humidity = int(data.get("humidity"))
        p_distance = int(data.get("distance"))
        p_volume = int(data.get("volume"))

        conn = get_db_connection()
        cur = conn.cursor()

        # PostgreSQL 함수 호출
        cur.execute(
            "SELECT insert_home_prj_data(%s, %s, %s, %s);",
            (p_insert_datetime, p_humidity, p_distance, p_volume)
        )
        
        conn.commit()
        cur.close()
        conn.close()

        return jsonify({"status": "success"})

    except Exception as e:
        return jsonify({"error": str(e)}), 500

@app.route("/api/get_home_prj_data", methods=["GET"])
def get_home_prj_data():
    # 쿼리 파라미터로 기간 받기 (예: ?start=2025-10-01T00:00:00&end=2025-10-18T23:59:59)
    start = request.args.get('start')
    end = request.args.get('end')
    if not start or not end:
        return jsonify({'error': 'start and end parameters required'}), 400

    conn = get_db_connection()
    cur = conn.cursor()
    cur.execute("SELECT * FROM get_home_prj_data(%s, %s)", (start, end))
    rows = cur.fetchall()
    columns = [desc[0] for desc in cur.description]
    cur.close()
    conn.close()

    result = [dict(zip(columns, row)) for row in rows]
    return jsonify(result)

# Flask 앱 실행
if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)

