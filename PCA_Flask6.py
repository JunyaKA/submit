from flask import Flask, request, jsonify
from flask_cors import CORS  # CORSを取り扱うためのライブラリをインポート
import numpy as np
from sklearn.decomposition import PCA
import csv

app = Flask(__name__) # Flaskインスタンスを生成。基本的な初期化。__name__は現在のモジュール名（.pyが抜けたファイル名）を示す特殊変数
CORS(app)  # CORSを適用させる事で、FlaskがUnityやフロントエンドのアクセスに対応できるようになる

# データをCSVファイルに書き出す関数
def write_to_csv(filename, data):
    data_np = np.array(data)
    max_values = np.max(data_np, axis=0) # 3列文の二次元データなので、それぞれの列の最大を取り一次元になる
    min_values = np.min(data_np, axis=0)
    diff_values = max_values - min_values

    with open(filename, 'w', newline='') as csvfile:  # 'w'モードで上書き。filenameが指定されたファイル名。newline=''適切な改行が挟まれる
    # 開いたファイルをcsvfileという変数で扱っている。withステートメントで書くとコードブロックが終了すると自動でファイルが閉じられる
        writer = csv.writer(csvfile) # csv.writer関数。csvファイルへ書き込みを行うためのオブジェクトを作成
        writer.writerow(max_values) # 1行として書き込む
        writer.writerow(min_values)
        writer.writerow(diff_values)
        writer.writerows(data_np) # 引数の二次元リストや配列をcsvファイルに複数行として書き込む

@app.route('/get_plane_points', methods=['POST']) # デコレータ。その関数の動作を修飾・拡張する
# http://<サーバのアドレス>/get_plane_points（これがエンドポイント）というURLにアクセスが有った時にのみ、このデコレータ直下の関数が実行される
# エンドポイントがPOSTメソッドのHTTPリクエストのみを受け付け。他にもメソッドは多数あり
def get_plane_points(): # エンドポイントにアクセスが有った時のみ実行される
    try:
        # Unityから送られてきたデータをログに出力
        # print(request.data)  # 受け取った生のデータを出力

        # Unityから送られてきたデータを受け取る
        data = request.json # requestオブジェクトはエンドポイントに送られてきたHTTPリクエスト全体。その中のjsonファイルにアクセス。今回は辞書形式
        points = np.array(data['points']) # 辞書なのでpointsキーに関連付けられたxyzのリストを取得し、numpyのndarrayに変換

        # Unityから送られてきたデータをCSVファイルに書き出す
        input_filename = "input_data.csv"
        write_to_csv(input_filename, points.tolist()) # ndarrayになっているpointsを通常のリストに戻している

        # 親指を動かした範囲を計算
        x_range = np.max(points[:, 0]) - np.min(points[:, 0]) # ２次元配列なので[:,0]で全てのx座標に対してソードしている
        y_range = np.max(points[:, 1]) - np.min(points[:, 1])
        z_range = np.max(points[:, 2]) - np.min(points[:, 2])
        
        # 3つの範囲の中で最大のものを選択
        max_range = max(x_range, y_range, z_range)

        # 動かした範囲の中心を計算
        center_x = (np.max(points[:, 0]) + np.min(points[:, 0])) / 2
        center_y = (np.max(points[:, 1]) + np.min(points[:, 1])) / 2
        center_z = (np.max(points[:, 2]) + np.min(points[:, 2])) / 2
        center_point = np.array([center_x, center_y, center_z])

        # 主成分分析（PCA）を使用して、最もよく近似する直線を求める
        pca = PCA(n_components=2) # PCAクラスを使用、主成分の数が1のオブジェクトを作成。2にしとくと2つ入る
        # ここでインスタンスを作成
        pca.fit(points) # 受け取ったpointsデータを適用

        direction = pca.components_[0] # PCAによって見つけられた各主成分の（原点からの）方向ベクトルを含む配列。０は最も大きい成分
        second_principal_component = pca.components_[1]  # 第二主成分
        print("First principal component:", direction)
        print("Second principal component:", second_principal_component)

        # # 第一成分と第二成分の外積を取り、それを法線として使用
        # normal_vector = np.cross(direction, second_principal_component)
        # normal_vector /= np.linalg.norm(normal_vector)  # 法線ベクトルを正規化

        mean = pca.mean_ # サンプル点全てのx座標の平均、y座標の平均、z座標の平均を計算して得られる、データ全体の「中心点」。directionはこの座標を通る

        # 中心を基準にして、direction ベクトルのスケーリング範囲を調整
        line_points = np.array([center_point + direction * t for t in np.linspace(-max_range/2, max_range/2, 100)])
        # line_points = np.array([mean + direction * t for t in np.linspace(-10, 10, 100)])
        # t : -10~10までを100分割。-10, -9.8, -9.6, ..., 9.8, 10のような数値のリスト
        # meanからdirection方向にtだけ移動した点を表している
        # print(line_points)
        # [[-2.17328127e-01 -6.36442804e-02 -8.59911534e-01]
        #  [-1.13947761e-01 -1.58938909e-02 -6.93044635e-01]
        #  [-1.05673948e-02  3.18564986e-02 -5.26177735e-01] ...100点？

        plane_points = []
        # width = 2
        width = max_range / 2  # ここで正方形の面のサイズを決定
        for i in range(len(line_points) - 1): # 隣接した２点をペアとするため99回ループ
            point1 = line_points[i]
            point2 = line_points[i + 1] # 現在の点と次の点

            # 第一成分と第二成分の外積を取り、それを法線として使用
            normal_vector = np.cross(direction, second_principal_component)
            normal_vector /= np.linalg.norm(normal_vector)  # 法線ベクトルを正規化

            offset = normal_vector * width  # 第二主成分の方向にoffsetを取る
            quad = [point1 + offset, point1 - offset, point2 - offset, point2 + offset]
            plane_points.append(quad)
            # normal_vector = np.cross(direction, second_principal_component)  # 新しい法線ベクトルの計算
            # normal_vector /= np.linalg.norm(normal_vector)  # 法線ベクトルを正規化

            # normal_vector = np.cross(direction, [0, 0, 1]) # 面の法線ベクトル
            # 外積を計算する関数。外積はdirectionに対しても[0, 0, 1]に対しても垂直。
            # directionに対し、どの角度からもう一辺が出ていてもdirectionに対して垂直になる
            # print(normal_vector)
            # [ 0.23636443 -0.51173281  0.        ]
            # [ 0.23636443 -0.51173281  0.        ]
            # [ 0.23636443 -0.51173281  0.        ] ... 同じ値が99点？
            # つまり１回だけなら1点の（法線の）ベクトル情報が計算される
            # normal_vector /= np.linalg.norm(normal_vector) # 法線ベクトルを正規化
            # linalg.norm() ユークリッドノルム（長さ）を計算。各成分の二乗和の平方根。
            # その正規化した値で各成分を割ってまた、割った成分のユークリッドノルムを取ると大きさが1になる
            # [ 0.41932149 -0.90783781  0.        ]が99点

            # offset = normal_vector * width
            # offset = [ 0.83864298 -1.81567562  0.        ]
            # offsetの値で主成分ベクトルとの距離を決められる

            # quad = [point1 + offset, point1 - offset, point2 - offset, point2 + offset]
            # 主成分ベクトル＋法線ベクトルの各点を作成
            # plane_points.append(quad)
            # print(plane_points)
            # [[array([ 0.62131485, -1.8793199 , -0.85991153]), 
            #   array([-1.0559711 ,  1.75203134, -0.85991153]), 
            #   array([-0.95259074,  1.79978173, -0.69304463]), 
            #   array([ 0.72469522, -1.83156951, -0.69304463])], 
            #   [array([ 0.72469522, -1.83156951, -0.69304463]), ...4点ずつ99回入っている
            # ずれていくのはfor文で次々と100個のlinepointsを読んでいくから

        # NumPyのndarrayをPythonのリストに変換
        plane_points = [[list(point) for point in quad] for quad in plane_points]

       # Unityに返すデータをCSVファイルに書き出す
        output_filename = "output_data.csv"
        flattened_plane_points = [point for quad in plane_points for point in quad]

        write_to_csv(output_filename, flattened_plane_points)

        # print(plane_points)
        # [[[0.6213148496777635, -1.87931990049635, -0.8599115342555024], 
        #   [-1.0559711041535467, 1.752031339618022, -0.8599115342555024], 
        #   [-0.9525907379467203, 1.7997817291218434, -0.6930446345735728], 
        #   [0.7246952158845897, -1.8315695109925285, -0.6930446345735728]], 
        #   [[0.7246952158845897, -1.8315695109925285, -0.6930446345735728], ...
        # list(point)でndarrayを単なるpythonリストに変換している。appendしてしまうとndarrayのまま
        # 標準リストはJSONファイルに変換できるが、ndarrayは変換できないため

        # Unityでの表示
    #      "plane_points": [[
    #   [ 0.6213148496777635,-1.87931990049635,-0.8599115342555024],
    #   [-1.0559711041535467,1.752031339618022,-0.8599115342555024],
    #   [-0.9525907379467203, 1.7997817291218434,-0.6930446345735728],
    #   [0.7246952158845897,-1.8315695109925285,-0.6930446345735728]],
    # [[ 0.7246952158845897,-1.8315695109925285,-0.6930446345735728], ...

        # 面の点をJSONとして返す
        return jsonify({"plane_points": plane_points}) # Flaskのjsonify関数。pythonオブジェクトをJSON形式のレスポンスに変換
        # {}内は辞書。"plane_points"キーの、plane_points変数。この変数が平面のリスト

    except Exception as e: # tryブロック中の例外処理。キャッチされた例外オブジェクトをeに代入
        print(str(e))
        return jsonify({"error": str(e)}), 400  # エラーメッセージとともに400ステータスコードを返す

if __name__ == '__main__': # 他のファイルからインポートされた場合、__name__はそのファイル名の.pyを取った名前が入る。その場合Flaskサーバーは不用意に起動しない
# つまり直接実行された時のみ以下が実行
    app.run(debug=True)