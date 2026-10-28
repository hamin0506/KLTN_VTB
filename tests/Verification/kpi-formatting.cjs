// Execute the actual KPI rendering loop with isolated formatting inputs.
const fs=require('fs'),vm=require('vm'),assert=require('assert');
const script=fs.readFileSync('wwwroot/js/dashboard.js','utf8');
const loop=script.slice(script.indexOf('for(const el of root.querySelectorAll'),script.indexOf(' const shared=',script.indexOf('function render(data)')));
const ids=['totalEmployees','completenessRate','missingEmployees','expiringDocuments','expiredDocuments','invalidDocuments'];
for(const value of [0,1,1234567890123,null]){
 const nodes=ids.map(id=>({dataset:{kpi:id},textContent:''}));const kpis=Object.fromEntries(ids.map(id=>[id,id==='completenessRate'&&value!==null?0.125:value]));
 vm.runInNewContext(loop,{root:{querySelectorAll:()=>nodes},data:{kpis},number:new Intl.NumberFormat('vi-VN'),percent:new Intl.NumberFormat('vi-VN',{style:'percent',minimumFractionDigits:1,maximumFractionDigits:1})});
 for(const node of nodes)assert.equal(node.textContent,value===null?'(Trống)':node.dataset.kpi==='completenessRate'?'12,5%':new Intl.NumberFormat('vi-VN').format(value));
}
console.log('PASS: actual rendering loop for all six KPI values: zero, one, long number, percent, BLANK. Isolated formatting inputs only; no SQL data replaced. Layout not tested.');
