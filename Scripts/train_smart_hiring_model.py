# Smart Hiring Random Forest Model Training Script
# Upload this to Azure ML Studio

import pandas as pd
import numpy as np
from sklearn.ensemble import RandomForestClassifier
from sklearn.model_selection import train_test_split, cross_val_score
from sklearn.preprocessing import StandardScaler, LabelEncoder
from sklearn.metrics import classification_report, confusion_matrix, roc_auc_score
import joblib
import json
from azureml.core import Run, Dataset, Model
from azureml.core.model import InferenceConfig
from azureml.core.webservice import AciWebservice
import argparse

def load_and_prepare_data():
    """Load training data from Azure ML datastore"""
    # In Azure ML Studio, replace this with your dataset
    # dataset = Dataset.get_by_name(workspace, 'smart_hiring_training_data')
    # df = dataset.to_pandas_dataframe()
    
    # For now, we'll create sample data structure based on your models
    # You'll replace this with actual data export from your database
    
    # Sample feature columns based on your database schema
    feature_columns = [
        'skill_match_score',      # Percentage of required skills freelancer has
        'avg_rating',            # Average rating from FreelancerFeedback
        'recommendation_rate',   # Percentage of WouldRecommend = true
        'completion_rate',       # Projects completed vs started
        'bid_success_rate',      # Accepted bids / total bids
        'category_experience',   # Number of projects in same category
        'response_time_hours',   # Average time to submit bid
        'portfolio_quality',     # Number of previous works + repositories
        'budget_match_score',    # How close bid is to project budget (0-1)
        'delivery_time_days',    # Proposed delivery time
        'freelancer_tenure_days', # How long freelancer has been on platform
        'project_complexity',    # Based on description length and requirements
        'client_history_score',  # Past collaboration success with same client
        'past_collaboration',    # 1 if worked together before, 0 otherwise
        'skills_count_match',    # Number of matching skills (absolute)
        'workload_factor'        # Current active projects for freelancer
    ]
    
    # Load your actual training data here
    # df = pd.read_csv('training_data.csv')
    
    return None  # Replace with actual dataframe

def engineer_features(df):
    """Feature engineering and preprocessing"""
    # Handle missing values
    df = df.fillna(0)
    
    # Create interaction features
    df['skill_budget_interaction'] = df['skill_match_score'] * df['budget_match_score']
    df['experience_rating_interaction'] = df['category_experience'] * df['avg_rating']
    
    # Normalize numerical features
    scaler = StandardScaler()
    numerical_features = [
        'skill_match_score', 'avg_rating', 'recommendation_rate', 
        'completion_rate', 'bid_success_rate', 'response_time_hours',
        'portfolio_quality', 'budget_match_score', 'delivery_time_days',
        'freelancer_tenure_days', 'project_complexity'
    ]
    
    df[numerical_features] = scaler.fit_transform(df[numerical_features])
    
    return df, scaler

def train_model(df):
    """Train Random Forest model"""
    # Separate features and target
    feature_columns = [col for col in df.columns if col != 'is_successful_match']
    X = df[feature_columns]
    y = df['is_successful_match']
    
    # Split data
    X_train, X_test, y_train, y_test = train_test_split(
        X, y, test_size=0.2, random_state=42, stratify=y
    )
    
    # Configure Random Forest
    rf_model = RandomForestClassifier(
        n_estimators=200,
        max_depth=15,
        min_samples_split=10,
        min_samples_leaf=5,
        max_features='sqrt',
        random_state=42,
        class_weight='balanced'  # Handle imbalanced classes
    )
    
    # Train model
    rf_model.fit(X_train, y_train)
    
    # Evaluate model
    train_score = rf_model.score(X_train, y_train)
    test_score = rf_model.score(X_test, y_test)
    
    # Cross-validation
    cv_scores = cross_val_score(rf_model, X_train, y_train, cv=5)
    
    # Predictions for detailed metrics
    y_pred = rf_model.predict(X_test)
    y_pred_proba = rf_model.predict_proba(X_test)[:, 1]
    
    print(f"Training Accuracy: {train_score:.4f}")
    print(f"Test Accuracy: {test_score:.4f}")
    print(f"Cross-validation Score: {cv_scores.mean():.4f} (+/- {cv_scores.std() * 2:.4f})")
    print(f"ROC AUC Score: {roc_auc_score(y_test, y_pred_proba):.4f}")
    
    print("\nClassification Report:")
    print(classification_report(y_test, y_pred))
    
    # Feature importance
    feature_importance = pd.DataFrame({
        'feature': feature_columns,
        'importance': rf_model.feature_importances_
    }).sort_values('importance', ascending=False)
    
    print("\nTop 10 Feature Importances:")
    print(feature_importance.head(10))
    
    return rf_model, feature_columns, feature_importance

def create_inference_script():
    """Create scoring script for deployment"""
    inference_script = '''
import json
import numpy as np
import pandas as pd
import joblib
from azureml.core.model import Model

def init():
    global model, feature_columns
    model_path = Model.get_model_path('smart_hiring_model')
    model = joblib.load(model_path)
    
    # Load feature columns
    with open(model_path.replace('.pkl', '_features.json'), 'r') as f:
        feature_columns = json.load(f)

def run(raw_data):
    try:
        # Parse input data
        data = json.loads(raw_data)['data']
        
        # Convert to DataFrame
        df = pd.DataFrame(data, columns=feature_columns)
        
        # Make predictions
        predictions = model.predict_proba(df)[:, 1]  # Probability of successful match
        
        # Return results
        result = {
            'predictions': predictions.tolist(),
            'model_version': '1.0'
        }
        
        return json.dumps(result)
    
    except Exception as e:
        return json.dumps({'error': str(e)})
'''
    
    with open('score.py', 'w') as f:
        f.write(inference_script)

def main():
    # Initialize Azure ML Run
    run = Run.get_context()
    
    # Load and prepare data
    print("Loading training data...")
    df = load_and_prepare_data()
    
    if df is None:
        print("No training data available. Please upload training data to Azure ML datastore.")
        return
    
    # Engineer features
    print("Engineering features...")
    df_processed, scaler = engineer_features(df)
    
    # Train model
    print("Training model...")
    model, feature_columns, feature_importance = train_model(df_processed)
    
    # Log metrics to Azure ML
    run.log('test_accuracy', model.score(
        df_processed.drop('is_successful_match', axis=1), 
        df_processed['is_successful_match']
    ))
    
    # Save model artifacts
    print("Saving model...")
    joblib.dump(model, 'smart_hiring_model.pkl')
    joblib.dump(scaler, 'feature_scaler.pkl')
    
    # Save feature columns
    with open('feature_columns.json', 'w') as f:
        json.dump(feature_columns, f)
    
    # Save feature importance
    feature_importance.to_csv('feature_importance.csv', index=False)
    
    # Create inference script
    create_inference_script()
    
    # Register model
    model_registered = Model.register(
        workspace=run.experiment.workspace,
        model_path='smart_hiring_model.pkl',
        model_name='smart_hiring_model',
        description='Random Forest model for smart freelancer hiring',
        tags={'version': '1.0', 'type': 'RandomForest'}
    )
    
    print(f"Model registered with ID: {model_registered.id}")
    print("Training completed successfully!")

if __name__ == '__main__':
    main()
'''

