from docx import Document

doc = Document('media-portfolio-backend/API-Contract-v1.0.docx')

table = doc.tables[11]
print("Table 11:")
for row in table.rows:
    print([c.text for c in row.cells])
