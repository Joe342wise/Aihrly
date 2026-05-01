## 1. Schema Question

**Tables:**

| Table | Key Columns |
|-------|-------------|
| `applications` | `id` (PK), `job_id` (FK → jobs), `candidate_name`, `candidate_email`, `stage`, `culture_fit_score`, `interview_score`, `assessment_score` (each with comment, updated_by, updated_at), `created_at` |
| `application_notes` | `id` (PK), `application_id` (FK → applications), `type`, `description`, `created_by` (FK → team_members), `created_at` |
| `stage_history` | `id` (PK), `application_id` (FK → applications), `from_stage`, `to_stage`, `changed_by` (FK → team_members), `reason`, `changed_at` |

Scores are stored as nullable integer columns directly on `applications` — simple for the current requirements (PUT overwrites the current value, no history needed yet).

**Indexes currently implemented:**
- `(job_id, candidate_email)` UNIQUE — enforces no duplicate applications per job at the DB level
- `(application_id)` on `application_notes` — FK lookup for profile and notes endpoints
- `(application_id)` on `stage_history` — FK lookup for profile endpoint

**Indexes I'd add beyond what's currently implemented:**
- `(job_id, created_at DESC)` on `applications` — composite to cover `GET /api/jobs/{jobId}/applications?stage=xxx` filter + sort in one seek
- `(application_id, created_at DESC)` on `application_notes` — same pattern for notes listing
- `(application_id, changed_at DESC)` on `stage_history` — same pattern for history ordering

**`GET /api/applications/{id}` query — 3 round-trips:**

```sql
-- 1: The application itself (scores are columns, zero extra cost)
SELECT * FROM applications WHERE id = $1;

-- 2: Notes with author names
SELECT n.*, tm.name AS author_name
FROM application_notes n
JOIN team_members tm ON n.created_by = tm.id
WHERE n.application_id = $1
ORDER BY n.created_at DESC;

-- 3: Stage history with changer names
SELECT sh.*, tm.name AS changed_by_name
FROM stage_history sh
JOIN team_members tm ON sh.changed_by = tm.id
WHERE sh.application_id = $1
ORDER BY sh.changed_at DESC;
```

3 round-trips is fine here. Each query hits a primary key or indexed foreign key, so they're fast. A single JOIN would create a cartesian product (notes × history rows) and produce duplicated data — the 3-query approach is the right tradeoff for correctness and readability.

---

## 2. Scoring Design Trade-off

### (a) Three separate endpoints vs. one generic endpoint

**Why three separate endpoints are better:**
- **Simpler validation** — each endpoint validates exactly one dimension. A generic endpoint would need to figure out which fields are present, validate each one, and decide if partial updates are allowed or if it's all-or-nothing.
- **Clearer authorization** — in a real system, only hiring managers might be allowed to set interview scores, while recruiters set culture fit. Separate endpoints make this natural.
- **Cleaner audit logs** — `PUT /scores/interview` is immediately clear. A generic endpoint requires reading the request body to understand what happened.

**When one generic endpoint would be better:**
- If the UI scores all dimensions at once and hits "Save" — one call instead of three reduces network round-trips.
- If score dimensions grow (e.g., 10+), ten separate endpoints become unwieldy and harder to maintain.

### (b) If we needed full score history

Scores as columns only hold the current value — history isn't possible without a new table. Each overwrite permanently loses the previous value. I'd create:

```sql
CREATE TABLE application_scores (
    id uuid PRIMARY KEY,
    application_id uuid NOT NULL REFERENCES applications(id),
    dimension varchar(20) NOT NULL,  -- 'culture_fit', 'interview', 'assessment'
    score integer NOT NULL,
    comment text,
    updated_by uuid NOT NULL REFERENCES team_members(id),
    updated_at timestamptz NOT NULL
);
```

The existing `PUT` endpoints stay the same — they'd insert a new row instead of overwriting a column. `GET /api/applications/{id}` would return the latest score per dimension via `ORDER BY updated_at DESC LIMIT 1`. No endpoint signature changes needed.

---

## 3. Debugging Question

A recruiter reports: "I moved a candidate to Interview yesterday and today the system says they're still in Screening."

1. **Check the database directly** — `SELECT stage FROM applications WHERE id = $1`. If it says "interview", the DB is correct and the bug is upstream (cache or UI). If it says "screening", the change never persisted.
2. **Check `stage_history`** — `SELECT * FROM stage_history WHERE application_id = $1 ORDER BY changed_at DESC`. Was a screening → interview transition ever recorded? Who did it and when?
3. **If no history record exists** — check server logs for `PATCH /api/applications/{id}/stage` from yesterday. Look for 4xx/5xx responses. A 400 could mean invalid transition or missing `X-Team-Member-Id` header.
4. **If history exists but the column is wrong** — something reverted it. Check for subsequent stage changes in `stage_history`, or a race condition where two concurrent PATCH requests overwrote each other.
5. **Check the client (browser)** — Open Network tab, reload the profile page. Does `GET /api/applications/{id}` return `"stage": "screening"`? If the API returns "interview" but the UI shows "screening", it's a frontend bug.
6. **Check Redis cache** — If the API returns "screening" but the DB says "interview", the cache is stale. Our cache invalidation should have cleared it on stage change, but a bug could have skipped it.
7. **Verify the `X-Team-Member-Id` header** — If the recruiter's request didn't include this header, it would have been rejected with 400. They might have missed the error and assumed it succeeded.

---

## 4. Honest Self-Assessment

| Skill | Rating | Reason |
|-------|--------|--------|
| C# | 3 | I'm comfortable with the language and ASP.NET Core fundamentals, but I'm still learning idiomatic patterns and advanced features. |
| SQL | 3 | Comfortable with CRUD, schema design, and basic queries, but still developing skills in advanced joins, indexing strategy, query optimization, and transactions. |
| Git | 3 | I use branching, merging, and basic rebasing daily, but I don't regularly use advanced tools like interactive rebase, bisect, or hooks. |
| REST API Design | 3 | I understand endpoints, auth, and structure, but I'm still growing in thinking about scalability, standards, and broader architecture. |
| Writing Tests | 3 | I can write meaningful unit and integration tests, but I want to get better at mocking external services, property-based testing, and writing fast, maintainable test suites. |
