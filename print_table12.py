from docx import Document

doc = Document('media-portfolio-backend/API-Contract-v1.0.docx')

table = doc.tables[12]
print("Table 12:")
for row in table.rows:
    print([c.text for c in row.cells])
