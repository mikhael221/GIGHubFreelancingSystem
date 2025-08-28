import pandas as pd
import joblib
import mysql.connector
from sklearn.ensemble import RandomForestClassifier
from sklearn.model_selection import train_test_split

def get_db_connection():
    return mysql.connector.connect(
        host="localhost",
        user="root",
        password="yourpassword",
        database="yourdbname"
    )

def load_data_from_mysql():
    conn = get_db_connection()
    query = "SELECT name, skills, budget, experience, hire FROM candidates"
    df = pd.read_sql(query, conn)
    conn.close()
    return df

def train_model():
    df = load_data_from_mysql()
    X = df[['skills', 'budget', 'experience']]
    y = df['hire']

    X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.3, random_state=42)

    clf = RandomForestClassifier(n_estimators=100, random_state=42)
    clf.fit(X_train, y_train)

    joblib.dump(clf, "rf_model.pkl")
    return clf

def predict_candidate(skills, budget, experience):
    clf = joblib.load("rf_model.pkl")
    X = [[skills, budget, experience]]
    prediction = clf.predict(X)[0]
    probability = clf.predict_proba(X)[0][1]
    return prediction, probability