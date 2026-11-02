const vm=require('vm'),fs=require('fs'),assert=require('assert');const ctx={window:{},Intl};vm.createContext(ctx);vm.runInContext(fs.readFileSync('wwwroot/js/donut-total.js','utf8'),ctx);const d=ctx.window.DonutTotal;
assert.equal(d.total([{value:31},{value:5},{value:2},{value:0}]),38);assert.equal(d.total([]),0);assert.equal(d.total([{value:null},{value:0}]),0);
for(const [w,h] of [[500,300],[320,300],[768,400]]){const g=d.graphic(38,w,h,.43,Math.min(w,h)*.22);assert.equal(g.length,1);assert.equal(g[0].x,w*.5);assert.equal(g[0].y,h*.43);assert.equal(g[0].style.text,'38');assert(g.every(x=>x.silent));assert(g[0].style.fontSize<=28);}
const js=fs.readFileSync('wwwroot/js/dashboard.js','utf8');assert(js.includes('distributionTotal=DonutTotal.total(data.documentDistribution)'));assert(js.includes('charts.forEach(c=>c.resize());updateDistributionTotal()'));
const css=fs.readFileSync('wwwroot/css/profile-print.css','utf8');assert(css.includes('@page document-catalog { size:A4 portrait; margin:9mm; }'));assert(css.includes('.document-catalog{page:document-catalog'));assert(!css.includes('transform:scale'));assert(css.includes('max-height:none;overflow:visible'));
assert(!fs.readFileSync('Views/Dashboard/Contract.cshtml','utf8').includes('Nhân viên cần xử lý hợp đồng theo đơn vị/phòng ban'));
console.log('PASS total bucket sum, filtered/empty inputs, exact donut center, silent graphics, responsive coordinates, scoped A4 CSS and chart title.');

for(const value of [0,33,425,123456789]){const g=d.graphic(value,390,300,.45,60);assert.equal(g.length,1);assert.equal(g[0].y,135);assert.equal(g[0].style.text,new Intl.NumberFormat('vi-VN').format(value));assert.equal(g[0].style.verticalAlign,'middle');}console.log('PASS number-only center, zero/large totals and both donut centers.');
