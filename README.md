# ZipApp

Программа создает файл с расширением ```.ai``` и для разжатия принимает только этот тип

Примеры запуска:

Для сжатия:
```console
foo@bar:~$ client.exe compress E:\1.txt E:\result.ai
```
Для разжатия:
```console
foo@bar:~$ client.exe decompress E:\result.ai E:\originalFile.txt
```


## Структура сжатого файла

<table>    
    <tr>    	
      <th>
          <p>Размер исходного файла</p>
        <table>
          <tr>
            <th>Индекс блока</th>
          </tr>
          <tr>
            <th>Размер сжатых данных</th>
          </tr>
          <tr>
            <th>Исходный размер данных</th>
          </tr>
            <tr>
            <th>Данные</th>
          </tr>
        </table>
        <table>
          <tr>
          ...
          </tr>
        </table>
        <table>
          <tr>
            <th>Индекс блока</th>
          </tr>
          <tr>
            <th>Размер сжатых данных</th>
          </tr>
          <tr>
            <th>Исходный размер данных</th>
          </tr>
             <tr>
            <th>Данные</th>
          </tr>
        </table>
      </th>
</table>
  
