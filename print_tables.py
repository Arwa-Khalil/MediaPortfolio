from docx import Document

doc = Document('media-portfolio-backend/API-Contract-v1.0.docx')

for i, table in enumerate(doc.tables):
    print(f"Table {i}:")
    try:
        print(f"  Row 0: {[c.text for c in table.rows[0].cells]}")
    except:
        pass
