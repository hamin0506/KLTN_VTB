# Ánh xạ nguồn PBIR sang hai Dashboard

Đọc toàn bộ visual.json; không chỉnh nguồn Power BI.

## Problem documents

| Visual ID | Loại | Trường/Measure | SHA256 |
|---|---|---|---|
| 066d855b69bdace5aab6 | tableEx | employee_id, full_name, document_name, document_group, issue_date, expiry_date, expiry_days, Trạng thái, invalid_reason | df7c3500e1aa8bf8d499e5e4180e2cb121e9f1b31f7b63fc2171bdf9cd7ef31f |
| 21dd8563db710ba0a6ab | slicer | division_name, department_name | c5df46049a1bc27efdb28fab55b33e9c36c0dc47e46e2cf004b43b1f99776fd0 |
| 26ef63fc6eb10ce1d93b | tableEx | employee_id, full_name, document_code, document_name, expiry_date, expiry_days | 3756c0962adcac0ec0a8e9b2870cd592407736247115c71243dfa77180d2d486 |
| 35ff735cc0b87dd00128 | cardVisual | Tỷ lệ đáp ứng hồ sơ | ebc34cc468e10824be7ff0c2b2e162554bb2944ea42a4f5d8c347c4fc1974d2c |
| 5bf0cc0a9912b5810d66 | slicer | document_group, document_code | 3487e8b4c4ec6715c6ead2b91d3e37ce9facdf3f1c86f1309d7108dd5f0ad195 |
| 629894d271525ca725c0 | clusteredBarChart | document_name, Hồ sơ đang thiếu | e4764c72e9c46e6c94a12fa1059ddf606e7fff63829df9e907292f4fd96a9346 |
| 7cf38fba544bdddb5956 | slicer | Employee Display | 2f251d3640af9468ce30643a8706705874ed8f39aaeb0e312dd734fbc0061c29 |
| 84db5208ed2b4506cc6e | cardVisual | Tổng yêu cầu hồ sơ | db46f2daed253f2842581b1631208048f1de8978a701db5ad63f9da2b62aad6a |
| 92e6efd7787e4d8ebcce | textbox |  | f310c9c863db71e3119743ef0c15071b24e442d959073f0e5b65c4342c25ee3b |
| b6d9dd60ae14ad55e0c1 | clusteredBarChart | department_name, Tỷ lệ đáp ứng hồ sơ | f4ceb185885d9b16de66abd39b33ea3bc3dab0d43290e78142d6eb265b92a2e8 |
| ed278dc9181c008dc49b | cardVisual | Hồ sơ đang thiếu | 98dca8e4f9628fcc3e1ca5e3295d2617ed79b10d324def2b68c4751cfdb2c9af |
| ee61c5a571bbb991783d | textbox |  | 5b1fd815503356b7ba53c53e6aace9add8310b1fff6ca411a612afb61698d782 |

## Contract

| Visual ID | Loại | Trường/Measure | SHA256 |
|---|---|---|---|
| 2f553c9a63a36ddba84a | cardVisual | Nhân viên có HDLD Hợp lệ | dabc73bfa43fa38b98fab12c6f81f391173b2fa631ce55347bafbd6f37a42d2f |
| 325e875dbe6a78a0163c | tableEx | employee_id, full_name, department_name, Trạng thái nhân viên, Trạng thái | 4ab247a42a8ea8990ee1aa19ab9f917e8ebf01f9cb8e3bbb21f33bb377ad0f11 |
| 7fbde19800460030e64c | textbox |  | 2d5c130acb6d71e6e47c2a4a81266364b3ddbcea00d12af9c04b6bb0825b3534 |
| 83e9bb3119130a83571d | slicer | Employee Display | 5824adad792af27c852df969121a820f2480d9a82b2df44e9fe7a3c4b82770ba |
| 8da888b3dd29a8455b09 | cardVisual | HĐ sắp hết hạn | ab307f7fa912fd737571e45da0d080286d5e334bc51f072c50e5443f8b911e4b |
| b40f36f796509e089500 | textbox |  | 6bb224c67ceea6478cc8569ec996ca5d4a06cb8eb6722e39ca516a314ac0b9e4 |
| bdcfbf9dc7b0b0960607 | tableEx | employee_id, full_name, document_code, document_name, issue_date, expiry_date, expiry_days, Trạng thái | d9d68743d4ae3cbb2f6063247c163ceee2c4ad9a3a5b2c373c4cc175e50c305e |
| bf3b621c3a79ba411be9 | barChart | department_name, Nhân viên cần xử lý HĐLĐ | c3c0894dbc973f1c9e562b00019589c12ee9df3fdf899a7d21f47807cea4c2db |
| c25dd44a2bd817b451c4 | cardVisual | Nhân viên thiếu HDXDTH1 | d95e694286c73277ff2707624f997ffd77a7c1a8250088dab05efda52e321719 |
| c30d82bdd53aa4bac706 | donutChart | Trạng thái, employee_id | 219cdd42e1afeaa0d5fd052b62c9fc14c3097ea68ca70185ea6b925bc72ad117 |
| cc849737a901b2080a90 | cardVisual | Nhân viên có HĐ cần ký tiếp | b14232fe130e4d0058c1008d77de1642d2a6d2c63884f4a68f44908eec594594 |
| e30a784466808cc2e570 | slicer | division_name, department_name | ae50d3e8e90596286b31e4d789aa96cef988068573138b80775fbc7562362792 |

## Chi tiết hợp đồng theo trạng thái

| Visual ID | Loại | Trường/Measure | SHA256 |
|---|---|---|---|
| b70343cc444fd27c1567 | actionButton |  | d019f40b0d37d7ff81fa71895e736f17d673c93b0860c08d7e0dccb666d6cbe2 |
| cbc3b38ec88834cd0a76 | tableEx | employee_id, full_name, Trạng thái nhân viên, department_name, Trạng thái | 34ac89ff79c5110591d700b67f978738dede46f8cb2a281fd1af4a43b01f47e7 |
