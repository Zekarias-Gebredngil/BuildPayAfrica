FROM node:20-alpine AS build
WORKDIR /app

# Install dependencies first (cached layer — only re-runs when package.json changes)
COPY frontend/package*.json ./
RUN npm ci

# Copy source (cache-busted whenever any source file changes)
COPY frontend/ .

# Remove any stale Angular build cache before building
RUN rm -rf .angular/cache

# Build production bundle
RUN npm run build

# ── Serve with Nginx ──────────────────────────────────────────────────────────
FROM nginx:alpine
# Angular 19 outputs to dist/frontend/browser/
COPY --from=build /app/dist/frontend/browser /usr/share/nginx/html
COPY docker/nginx/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
