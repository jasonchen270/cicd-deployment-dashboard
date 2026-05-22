# Deployment

## Architecture

```
   ┌─────────────┐
   │  Route 53   │
   └──────┬──────┘
          │
   ┌──────▼──────┐
   │     ALB     │ ── /api/* ──► ECS Service: api (port 5080)
   │ (public)    │
   │             │ ── /*     ──► ECS Service: frontend (port 8080)
   └─────────────┘
```

## AWS resources

- **ECR repos**: `cicd-dashboard-api`, `cicd-dashboard-frontend`
- **ECS cluster**: `cicd-dashboard` (Fargate)
- **Services**: `api`, `frontend` (each with desired count = 2 for HA)
- **ALB**: listener on 443 with TLS, two target groups, path-based routing
- **CloudWatch Logs**: `/ecs/cicd-dashboard-api`, `/ecs/cicd-dashboard-frontend`

## Bootstrap (one-time)

```bash
# Create ECR repos
aws ecr create-repository --repository-name cicd-dashboard-api
aws ecr create-repository --repository-name cicd-dashboard-frontend

# Create ECS cluster
aws ecs create-cluster --cluster-name cicd-dashboard --capacity-providers FARGATE

# Register task definitions
aws ecs register-task-definition --cli-input-json file://ecs-task-api.json
aws ecs register-task-definition --cli-input-json file://ecs-task-frontend.json

# Create services (after ALB target groups exist)
aws ecs create-service \
  --cluster cicd-dashboard \
  --service-name api \
  --task-definition cicd-dashboard-api \
  --desired-count 2 \
  --launch-type FARGATE \
  --network-configuration "awsvpcConfiguration={subnets=[subnet-aaa,subnet-bbb],securityGroups=[sg-xxx],assignPublicIp=ENABLED}" \
  --load-balancers "targetGroupArn=arn:aws:elasticloadbalancing:...,containerName=api,containerPort=5080"
```

## Continuous deployment

`.github/workflows/ci.yml` handles the full pipeline on every push to `main`:

1. xUnit + Karma tests run in parallel
2. Docker images built and pushed to ECR (tagged with commit SHA)
3. `aws ecs update-service --force-new-deployment` triggers a rolling deploy

The workflow uses **OIDC role assumption** (no long-lived AWS keys in GitHub). The role ARN comes from `secrets.AWS_DEPLOY_ROLE_ARN`.
