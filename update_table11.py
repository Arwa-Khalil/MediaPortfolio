from docx import Document

doc = Document('media-portfolio-backend/API-Contract-v1.0.docx')

table = doc.tables[11]

# Add cloudflareThumbnailImageId if it doesn't exist
has_thumb_id = False
has_stream_id = False
for row in table.rows:
    if 'cloudflareThumbnailImageId' in row.cells[0].text:
        has_thumb_id = True
    if 'cloudflareStreamId' in row.cells[0].text:
        has_stream_id = True

if not has_thumb_id:
    row1 = table.add_row()
    row1.cells[0].text = 'cloudflareThumbnailImageId'
    row1.cells[1].text = 'string (nullable)'
    row1.cells[2].text = 'Cloudflare image ID for the custom thumbnail'

if not has_stream_id:
    row2 = table.add_row()
    row2.cells[0].text = 'cloudflareStreamId'
    row2.cells[1].text = 'string'
    row2.cells[2].text = 'Cloudflare Stream ID for the video'

doc.save('media-portfolio-backend/API-Contract-v1.0.docx')
print("Document updated successfully.")
