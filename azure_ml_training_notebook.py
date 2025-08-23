# Smart Hiring Random Forest Training for Azure ML
# Copy this entire script into a Jupyter notebook in Azure ML Studio

import pandas as pd
import numpy as np
from sklearn.ensemble import RandomForestClassifier
from sklearn.model_selection import train_test_split
from sklearn.metrics import accuracy_score, classification_report, confusion_matrix
from azureml.core import Workspace, Dataset, Run
import joblib
import matplotlib.pyplot as plt
import seaborn as sns

print("🤖 Smart Hiring Random Forest Training Started!")
print("=" * 50)

# Get the current run context
run = Run.get_context()

# Load the dataset
try:
    # If running in Azure ML
    ws = run.experiment.workspace
    dataset = Dataset.get_by_name(ws, name='smart_hiring_training_data')
    df = dataset.to_pandas_dataframe()
    print(f"✅ Dataset loaded from Azure ML: {df.shape}")
except:
    # Fallback for local testing
    print("⚠️ Could not load from Azure ML, using local file...")
    df = pd.read_csv('smart_hiring_training_data_20250823.csv')

print(f"📊 Dataset shape: {df.shape}")
print(f"📋 Features: {df.columns.tolist()}")

# Data overview
print("\n📈 Data Overview:")
print(df.describe())

print(f"\n🎯 Target distribution:")
print(df['is_successful_match'].value_counts())

# Prepare features and target
feature_columns = [col for col in df.columns if col != 'is_successful_match']
X = df[feature_columns]
y = df['is_successful_match']

print(f"\n🔧 Features ({len(feature_columns)}): {feature_columns}")

# Split the data
X_train, X_test, y_train, y_test = train_test_split(
    X, y, test_size=0.2, random_state=42, stratify=y
)

print(f"\n📊 Train set: {X_train.shape}, Test set: {X_test.shape}")

# Train Random Forest model (same hyperparameters as your app)
print("\n🌲 Training Random Forest Model...")

rf_model = RandomForestClassifier(
    n_estimators=200,
    max_depth=15,
    min_samples_split=10,
    min_samples_leaf=5,
    max_features='sqrt',
    random_state=42,
    class_weight='balanced',
    verbose=1
)

rf_model.fit(X_train, y_train)

# Evaluate model
print("\n📊 Model Evaluation:")
y_pred = rf_model.predict(X_test)
y_pred_proba = rf_model.predict_proba(X_test)[:, 1]

accuracy = accuracy_score(y_test, y_pred)
print(f"✅ Model Accuracy: {accuracy:.4f}")

print("\n📋 Classification Report:")
print(classification_report(y_test, y_pred))

# Confusion Matrix
cm = confusion_matrix(y_test, y_pred)
plt.figure(figsize=(8, 6))
sns.heatmap(cm, annot=True, fmt='d', cmap='Blues', 
            xticklabels=['Not Successful', 'Successful'],
            yticklabels=['Not Successful', 'Successful'])
plt.title('Confusion Matrix')
plt.ylabel('Actual')
plt.xlabel('Predicted')
plt.show()

# Feature importance
feature_importance = pd.DataFrame({
    'feature': feature_columns,
    'importance': rf_model.feature_importances_
}).sort_values('importance', ascending=False)

print("\n🔝 Top 10 Most Important Features:")
print(feature_importance.head(10))

# Plot feature importance
plt.figure(figsize=(12, 8))
top_features = feature_importance.head(10)
plt.barh(range(len(top_features)), top_features['importance'])
plt.yticks(range(len(top_features)), top_features['feature'])
plt.xlabel('Feature Importance')
plt.title('Top 10 Feature Importance - Random Forest')
plt.gca().invert_yaxis()
plt.tight_layout()
plt.show()

# Log metrics to Azure ML
run.log('accuracy', accuracy)
run.log('train_samples', len(X_train))
run.log('test_samples', len(X_test))

for idx, (feature, importance) in enumerate(feature_importance.head(5).values):
    run.log(f'feature_importance_{idx+1}', importance)
    run.log(f'feature_name_{idx+1}', feature)

# Save the model
model_filename = 'smart_hiring_rf_model.pkl'
joblib.dump(rf_model, model_filename)
print(f"\n💾 Model saved as: {model_filename}")

# Upload the model to Azure ML
run.upload_file(name='outputs/' + model_filename, path_or_stream=model_filename)
print("✅ Model uploaded to Azure ML!")

# Test prediction function (same format as your app expects)
def predict_freelancer_success(features_dict):
    """
    Predict freelancer success probability
    Input: dict with feature names and values
    Output: probability score (0-1)
    """
    # Convert dict to DataFrame with same column order
    feature_df = pd.DataFrame([features_dict])[feature_columns]
    prob = rf_model.predict_proba(feature_df)[0, 1]
    return float(prob)

# Test with sample data
sample_features = {
    'skill_match_score': 0.85,
    'avg_rating': 4.5,
    'recommendation_rate': 0.9,
    'completion_rate': 0.95,
    'bid_success_rate': 0.3,
    'category_experience': 8,
    'response_time_hours': 12.0,
    'portfolio_quality': 7.5,
    'budget_match_score': 0.8,
    'delivery_time_days': 7.0,
    'freelancer_tenure_days': 500.0,
    'project_complexity': 6.0,
    'client_history_score': 0.6,
    'past_collaboration': 0,
    'skills_count_match': 4,
    'workload_factor': 0.3
}

test_prediction = predict_freelancer_success(sample_features)
print(f"\n🧪 Test Prediction: {test_prediction:.4f} ({test_prediction*100:.1f}%)")

print("\n🎉 Training Complete! Model ready for deployment.")
print("=" * 50)


