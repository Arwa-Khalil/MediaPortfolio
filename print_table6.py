from docx import Document

doc = Document('media-portfolio-backend/API-Contract-v1.0.docx')

table = doc.tables[6]
print("Table 6 (GET /videos?):")
for row in table.rows:
    print([c.text for c in row.cells])
