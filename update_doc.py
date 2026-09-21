import docx
import sys

def update_doc(path):
    doc = docx.Document(path)
    for paragraph in doc.paragraphs:
        if "201" in paragraph.text and ("video-upload-url" in paragraph.text or "image-upload-url" in paragraph.text or "Upload URL" in paragraph.text or "Returns 201" in paragraph.text):
            # This is a bit brittle, so let's just do a string replace across all text in paragraphs/tables
            pass
            
    # Actually, let's just do a blanket replacement in paragraphs and tables 
    # where we specifically find the text for upload endpoints
    
    for p in doc.paragraphs:
        if "201 Created" in p.text and ("video-upload-url" in p.text or "image-upload-url" in p.text):
            p.text = p.text.replace("201 Created", "200 OK")
        if "201" in p.text and ("video-upload-url" in p.text or "image-upload-url" in p.text):
             p.text = p.text.replace("201", "200")
             
    for table in doc.tables:
        for row in table.rows:
            for cell in row.cells:
                # We need to specifically replace the response code for the upload endpoints
                # If a cell contains "video-upload-url" or "image-upload-url", the adjacent cells might have the status code.
                # Let's just look at the whole cell text. If it's a response block.
                if "201 Created" in cell.text:
                    cell.text = cell.text.replace("201 Created", "200 OK")
                elif "201" in cell.text:
                    # check if the table/row has context of upload url
                    cell.text = cell.text.replace("201", "200")
                    
    doc.save(path)
    print("Done")

if __name__ == "__main__":
    update_doc(sys.argv[1])
