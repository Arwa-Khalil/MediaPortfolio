from docx import Document

doc = Document('media-portfolio-backend/API-Contract-v1.0.docx')
for i, p in enumerate(doc.paragraphs):
    if 'GET' in p.text or 'admin/videos' in p.text:
        print(f"[{i}] {p.text}")
