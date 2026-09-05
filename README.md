# Suttor Library

Suttor is a full-featured, AI-powered digital library backend API built with ASP.NET Core (.NET 10). It provides a complete ecosystem for readers, authors, and administrators — combining a rich book management system with a machine-learning recommendation engine, data-mining analytics dashboards, a social blog platform, an AI conversational assistant, and real-time push notifications.

---

## 🏗️ Architecture Overview

The platform follows a clean Repository / Unit of Work pattern with a clear separation between the relational and NoSQL data stores:

| Concern | Technology |
| --------- | ------------ |
| API Framework | ASP.NET Core (.NET 10) |
| Relational Database | MySQL via Entity Framework Core |
| Document Databases | MongoDB (recommendations, blog, AI conversations, statistics) |
| Machine Learning | ML.NET (Matrix Factorization, K-Means, Apriori) |
| Authentication | ASP.NET Core Identity + JWT Bearer tokens + Refresh Tokens |
| Push Notifications | Firebase Cloud Messaging (FCM) |
| API Documentation | OpenAPI + Scalar UI |
| File Handling | Multi-format book file upload/download (up to 100 MB) |

---

## 🔐 Authentication & User Management

• Full registration / login flow backed by ASP.NET Core Identity with configurable password policy (8+ chars, mixed case, digits, unique emails).

• JWT access tokens (HMAC-SHA256, validated issuer/audience/lifetime) paired with refresh tokens persisted to MySQL for seamless session renewal.

• Role-based authorization with built-in User and Author roles; users can register as authors.

• Password change / reset flows with proper exception handling.

• Custom JWT events: structured 401/403 JSON responses, authentication failure logging, and token-validated hooks.

• Gamification: users accumulate XP points through platform engagement.

• User profiles track full name, join date, author status, roles, and profile data.

---

## 📖 Book & Library Management

• Complete CRUD for Books, Authors, Categories, and Languages.

• Many-to-many relationships: BookAuthors, BookCategories.

• Book ratings (per-user, per-book with a default of 0).

• Favorites: users can bookmark favorite books.

• Downloads tracking: every download is recorded (Download entity) with a timestamp and an IsFinishReading flag — the raw signal that powers the recommendation engine.

• User interests: users declare preferred categories (UserInterests), used for cold-start recommendations.

• File upload/download service supporting large book files (100 MB multipart limit) with automatic download logging, file type resolution, and cleanup.

• Author model supports verification (IsRegistered) and profile pictures.

---

## 🤖 Recommendation System (Core Feature)

The platform ships with a production-grade, hybrid recommendation engine built from the ground up. It combines collaborative filtering via matrix factorization with an interest-based fallback to solve the cold-start problem at every level.

### How It Works

The system operates as an end-to-end pipeline: Extract → Train → Score → Persist.

          MySQL (Downloads + Interests)
                    │
                    ▼
      ┌───────────────────────-────────┐
      │  Matrix Factorization (ML.NET) │  ← Users with ≥ 3 downloads
      │  (Implicit/One-Class MF)       │
      └─────────────┬──────────────────┘
                    │ fallback if insufficient data
                    ▼
      ┌─────────────────────────────┐
      │  Interest-Based Fallback    │  ← New / low-activity users
      │  (Category similarity +     │
      │   popularity ranking)       │
      └─────────────┬───────────────┘
                    │
                    ▼
      MongoDB (per-user recommendation documents)

### 1. Matrix Factorization Recommender (Primary Engine)

• Uses ML.NET's MatrixFactorizationTrainer with the SquareLossOneClass loss function — designed for implicit feedback (downloads, not explicit star ratings). Every download is a positive signal; unobserved pairs are treated as unobserved rather than negative.

• Hyperparameters: Alpha=0.01, Lambda=0.025, C=0.00001, ApproximationRank=32, NumberOfIterations=20 — tuned as standard starting points for implicit-feedback problems.

• String user/book IDs are encoded to numeric keys via MapValueToKey transforms before training.

• Training gates prevent noise:

• System-wide: training is skipped entirely if fewer than 50 total interactions exist (MinTotalInteractionsToTrain).

• Per-user: MF scoring is used only for users with ≥ 3 downloads (MinDownloadsForMf).

• Batch scoring: all candidate (user, book) pairs are scored in a single Transform call — dramatically cheaper than per-pair PredictionEngine calls when generating recommendations for the full user base.

• Only books present in the training interactions are scored (the classic item cold-start limit of collaborative filtering); brand-new books surface through other channels until they accumulate downloads.

• Already-downloaded books are excluded from each user's candidates, ensuring recommendations are always fresh.

• The top 10 scored books per user are kept (TopN = 10).

### 2. Interest-Based Fallback Recommender (Cold-Start Handler)

Activated when the MF model can't produce trustworthy results for a user:

• If the user has declared interests (categories): finds all other users sharing those interests, collects the books they downloaded, excludes books the target user already has, and ranks candidates by download count within the similar-user cohort.

• If the cohort is too small to fill the top-N list, it tops up with globally popular books (system-wide download counts).

• If the user has no declared interests at all: goes straight to system-wide popularity ranking.

• The fallback score is a raw download count (a popularity rank), not an ML prediction.

### 3. Recommendation Pipeline (Orchestrator)

The RecommendationPipelineService ties everything together:

1. Extracts all download interactions, user interests, and user IDs from MySQL.

2. Trains the MF model (or gets null if insufficient system-wide data).

3. Iterates every user:

    • If the model exists and the user meets the download threshold → scores via Matrix Factorization.

    • Otherwise → routes through the Interest-Based Fallback.

4. Upserts results into MongoDB as BookRecommendationDocument records, each tagged with its source ("MatrixFactorization" or "InterestFallback") and a UTC timestamp.

### 4. Background Job (Scheduled Retraining)

• A BackgroundService runs the pipeline on a configurable timer (default: every 24 hours, configurable via Recommendations:IntervalHours).

• Creates a fresh DI scope per run to avoid captive-dependency issues with scoped services (DbContext, IUnitOfWork).

• Failures are logged but never crash the background loop — the next interval retries automatically.

• Can also be triggered on-demand via POST /api/recommendations/train.

### 5. MongoDB Recommendation Store

• Recommendations live in a dedicated MongoDB collection with per-user documents:

    • UserId, RecommendedBooks (book ID + score list), Source, GeneratedAtUtc.

• Upsert semantics: each run replaces the user's existing recommendations atomically — no stale data accumulates.

---

## 📊 Analytics & Data Mining Dashboards

A rich analytics layer provides three distinct dashboards and several data-mining algorithms:

### User Dashboard

Personal reading stats, download history, and a "Because You Read" feature that explains why a book was recommended by tracing it back to source books the user downloaded.

### Author Dashboard

Per-author performance metrics: downloads, ratings, top books.

### Admin Dashboard

• Platform overview and growth metrics (configurable month window, default 12).

• Most active users and top books/authors leaderboards.

• K-Means User Segmentation: clusters users into behavioral segments (default k=3) based on extracted feature vectors.

• Apriori Association Rules: discovers book-level and category-level "users who downloaded X also downloaded Y" rules with configurable minimum support (2%) and confidence (30%).

• Anomaly Detection: flags statistically anomalous users using a configurable z-score threshold (default 3σ) for fraud/abuse monitoring.

---

## 📝 Blog & Community Platform

• Full blog system backed by MongoDB — posts, comments, and likes.

• Rich content support with CRUD operations for posts and threaded comments.

• MongoDB index management via MongoIndexConfig for query performance.

---

🔔 Push Notifications (Firebase Cloud Messaging)

• Firebase Admin SDK integration for server-side notification dispatch.

• FCM token management: users register device tokens; tokens are stored per-user in MySQL.

• Notification logging via FcmLog and FcmUserLog entities for delivery auditing.

---

## ⚙️ Infrastructure & Cross-Cutting Concerns

• CORS: configurable allowed origins with credentials, exposed Content-Disposition header for file downloads, and 10-minute preflight caching.

• Form limits: tuned for large book uploads (100 MB value/multipart limits).

• Global exception handling middleware for consistent error responses.

• JWT key validation at startup — enforces ≥ 256-bit keys for HMAC-SHA256.

• Health checks, forwarded-headers support, and static file serving (with content-type mapping for ebook formats).

---

## 🗃️ Data Model Summary

| Entity | Store | Purpose |
| ------ | ------ | ------- |
| AppUser | MySQL | Identity user with XP, author flag, roles |
| Book / Author / Category / Languages | MySQL | Core library catalog
| BookRating | MySQL | Per-user star ratings |
| Download | MySQL | Download events (recommendation signal) |
| UserInterests | MySQL | Declared category preferences |
| FavoriteBooks | MySQL | User bookmarks |
| RefreshToken | MySQL | Auth token rotation |
| FCMToken / FcmLog / FcmUserLog | MySQL | Push notification infrastructure |
| BookRecommendationDocument | MongoDB | Per-user ML recommendations |
| Blog Post / Comment | MongoDB | Community content |
| Conversation / Message | MongoDB | AI chat history |

---

## 💬 AI Conversational Assistant

You can add a conversation in your Front-End with integrated A.I. ChatBot.

• Integrated AI chat service with conversation persistence in MongoDB.

• Supports multi-message conversations with titles, per-user history, and message threading.

• Separate MongoDB database configuration (AiDbSettings) for isolation.
