import os
import re
import PyPDF2
import easygui
from xml.etree.ElementTree import XML
import zipfile
import time


start_time = time.time()

inputDir = easygui.diropenbox()
listdir = [os.path.join(inputDir, file) for file in os.listdir(inputDir)]


for idx, inputFile in enumerate(listdir, start=1):
    file_name, file_extension = os.path.splitext(inputFile)

    if file_extension == '.txt':
        # текстовый файл
        fileTxt = open(inputFile, encoding='utf-8')
        data = fileTxt.read()
        fileTxt.close()

    elif file_extension == '.docx':  # док-файл
        xml_namespace = '{http://schemas.openxmlformats.org/wordprocessingml/2006/main}'
        para = xml_namespace + 'p'
        text = xml_namespace + 't'

        def getTextDocx(filenameDocx):
            file_zip = zipfile.ZipFile(filenameDocx)
            doc_xml = file_zip.read('word/document.xml')
            file_zip.close()
            tree = XML(doc_xml)
            paragraphs = []
            for paragraph in tree.iter(para):
                texts = [node.text
                         for node in paragraph.iter(text)
                         if node.text]
                if texts:
                    paragraphs.append(''.join(texts))

            return '\n'.join(paragraphs)
        data = getTextDocx(inputFile)

    elif file_extension == '.pdf':  # пдф-файл
        filePDF = open(inputFile, 'rb')
        pdfReader = PyPDF2.PdfReader(filePDF)
        data = ''
        for i in range(len(pdfReader.pages)):
            page = pdfReader.pages[i]
            data += page.extract_text()

    # поиск ключевого слова
    keyWords = 'апрель|май'
    matches = [m.span() for m in re.finditer(keyWords, data)]
    if matches:
        print("{}: ".format(file_name), matches)

end_time = time.time()

print(f"Время выполнения программы: {end_time - start_time} секунд")