# 圖片標記功能設計

## 緣起

面板製作過程中，有些檢查站需要作業員檢視玻璃影像，判斷是否有瑕疵。
需要一個系統能將玻璃影像縮放，又能正確紀錄在原始影像中瑕疵點的位置。

## 版面配置

功能列 - 放大鏡/縮小鏡/將圖片縮放回原影像區等寬(或高)的預設版本

影像顯示框

側邊列表選擇框 - 從GlassMarkers資料表讀取待判讀的圖片清單
- 選擇項目改變: 提示是否儲存該影像複選框選取的標記，並根據圖片ID更新影像顯示區

側邊複選框 - 從GlassMarkers資料表中，讀取該影像ID既有標記的點並構成複選框
- 取消選取: 在影像顯示區移除該標記點
- 選取: 在影像顯示區顯示該標記點
- 儲存: 儲存該影像複選框選取的標記
- 取消: 放棄該次在影像顯示區的標記，回復成原資料庫版本

![750x425_default(1)](https://github.com/simonwu513/GlassExamination/blob/main/Layout.png)

## 完成功能

滑鼠單擊 - 放大鏡/縮小鏡/預設鼠標
- 放大鏡: 以點擊位置為中心放大影像
- 縮小鏡: 以點擊位置為中心縮小影像
- 預設鼠標: 滑鼠長按可以點擊位置為中心，進行影像拖曳

滑鼠雙擊 - 標記瑕疵點位置，以原始圖片大小的長寬為絕對座標來儲存標記點

滑鼠在影像區上移動，顯示以下訊息:
- 圖片縮放長寬調整後的座標
- 以原始圖片大小的長寬為絕對座標的座標
- 縮放倍數為目前圖片顯示長/寬為原始圖片大小的倍率
- 若圖片經縮放，原點不會再是圖片顯示框左上角(0,0)位置，offset即為經縮放調整後在圖片顯示框外的原點

## 影像來源
pexel: 範例影像
fontawesome: 功能列、表單icon




