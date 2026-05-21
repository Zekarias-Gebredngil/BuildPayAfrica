-- BuildPay Africa — PostgreSQL initialisation
-- Runs once on first container start (empty volume).
-- Creates all service databases. The main app DB (buildpay_db) is
-- created automatically by POSTGRES_DB in docker-compose.

SELECT 'CREATE DATABASE keycloak_db'
  WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'keycloak_db')\gexec

SELECT 'CREATE DATABASE identity_db'
  WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'identity_db')\gexec

SELECT 'CREATE DATABASE marketplace_db'
  WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'marketplace_db')\gexec

SELECT 'CREATE DATABASE project_db'
  WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'project_db')\gexec

SELECT 'CREATE DATABASE payment_db'
  WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'payment_db')\gexec

SELECT 'CREATE DATABASE delivery_db'
  WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'delivery_db')\gexec

SELECT 'CREATE DATABASE notification_db'
  WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'notification_db')\gexec

SELECT 'CREATE DATABASE userprofile_db'
  WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'userprofile_db')\gexec
