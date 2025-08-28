from flask import Flask, render_template, request
from Models.RandomForest import train_model, predict_candidate

app = Flask(__name__)

# Train model when app starts (only once)
train_model()

@app.route("/")
def index():
    return render_template("index.html")

@app.route("/predict", methods=["POST"])
def predict():
    skills = int(request.form["skills"])
    budget = int(request.form["budget"])
    experience = int(request.form["experience"])

    prediction, probability = predict_candidate(skills, budget, experience)

    return render_template("result.html",
                           prediction=prediction,
                           probability=round(probability*100, 2))