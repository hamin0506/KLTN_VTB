const fs=require('fs'),vm=require('vm'),assert=require('assert');const src=fs.readFileSync('wwwroot/js/reports.js','utf8');
const context={number:new Intl.NumberFormat('vi-VN')};vm.createContext(context);vm.runInContext(src.slice(src.indexOf('function reportDeadline('),src.indexOf('class Table{')),context);
assert.equal(context.reportDeadline({expiryDate:'2020-01-01',days:-906}),'Quá hạn 906 ngày');assert.equal(context.reportDeadline({expiryDate:'2026-11-01',days:5}),'Còn 5 ngày');assert.equal(context.reportDeadline({expiryDate:'2026-11-01',days:0}),'Còn 0 ngày');assert.equal(context.reportDeadline({expiryDate:null,days:-20}),'—');assert.equal(context.reportDeadline({expiryDate:'2026-01-01',days:null}),'—');
assert(src.includes("expiring:'Days'"));assert(src.includes("this.kind==='problem'&&key==='Status'"));
console.log('PASS deadline presentation: expired, approaching, today, no expiry and unknown; ascending Days default.');
