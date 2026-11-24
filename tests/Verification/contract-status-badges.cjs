// Exercise the actual shared palette and badge renderer, without production data.
const fs=require('fs'),vm=require('vm'),assert=require('assert');
const source=fs.readFileSync('wwwroot/js/reports.js','utf8');
const ctx={document:{createElement(){return {style:{setProperty(k,v){this[k]=v;}}};}}};vm.createContext(ctx);
vm.runInContext(source.slice(source.indexOf('const stateColors='),source.indexOf('const badgeColors=')),ctx);
const cases=[['HOP_LE','Hợp lệ','#059669'],['THIEU_HDXDTH1','Thiếu HĐXĐTH1','#DC2626'],['CAN_KY_KET_TIEP','Cần ký kết tiếp','#D97706'],['KHONG_CANH_BAO','Không cảnh báo','#64748B']];
for(const phase of ['donut','state-link','filter-reload','next-page'])for(const [code,label,color] of cases){const badge=ctx.contractStatusBadge(code,label);assert.equal(badge.style['--contract-status-hue'],color);assert.equal(badge.textContent,label);assert(badge.className.includes('contract-status-badge'));assert(badge.style['--contract-status-ink']);}
assert.equal(ctx.contractStatusBadge('UNKNOWN','Khác').style['--contract-status-hue'],undefined);
assert(source.includes("(this.kind==='contractDetail'||this.kind==='contractIssues')&&key==='Status'"));
assert(!source.includes("key==='Employment'){td.append(contractStatusBadge"));
const service=fs.readFileSync('Services/DashboardService.Reports.cs','utf8');assert(service.includes('ContractStatusCode=c.LaborContractStatus'));
console.log('PASS four contract codes match existing chart hues; badge recreation after filter/page, neutral fallback, scope limited to contractDetail Status. Renderer test, not live browser.');

for(const [code,ink,bg] of [['CAN_KY_KET_TIEP','#92400E','#FFFAEB'],['THIEU_HDXDTH1','#991B1B','#FFF4F4']]){const badge=ctx.contractStatusBadge(code,'Nhãn');assert.equal(badge.style['--contract-status-ink'],ink);assert.equal(badge.style['--contract-status-background'],bg);}
console.log('PASS exact requested amber/red foregrounds and backgrounds by code.');

for(const [code,rgb] of [['CAN_KY_KET_TIEP','255, 250, 235'],['THIEU_HDXDTH1','255, 244, 244']]){
 const hex=ctx.contractStatusBadge(code,'Nhãn').style['--contract-status-background'];
 assert.equal(hex.slice(1).match(/../g).map(x=>parseInt(x,16)).join(', '),rgb);
}
console.log('PASS configured background RGB values (not browser computed styles).');

for(const phase of ['initial','search reload','sort reload'])for(const [code,label,ink,bg] of [['CAN_KY_KET_TIEP','Cần ký kết tiếp','#92400E','#FFFAEB'],['THIEU_HDXDTH1','Thiếu HĐXĐTH1','#9A3412','#FFF4F4']]){
 const badge=ctx.contractStatusBadge(code,label,ink);assert.equal(badge.style['--contract-status-ink'],ink);assert.equal(badge.style['--contract-status-background'],bg);
}
assert(source.includes("this.kind==='contractIssues'?badgeColors[value]:undefined"));
console.log('PASS contractIssues re-render uses shared code-based backgrounds and preserves original text colors. Simulated renderer only.');
