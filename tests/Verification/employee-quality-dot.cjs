const fs=require('fs'),vm=require('vm'),assert=require('assert');
const src=fs.readFileSync('wwwroot/js/dashboard.js','utf8');const start=src.indexOf("const a=document.createElement('a');a.href='/Profiles/Details/'");const code=src.slice(start,src.indexOf('td.append(a);',start)+'td.append(a);'.length);
const output=[];vm.runInNewContext(code,{row:{employeeId:'TEST0001'},td:{append:x=>output.push(x)},document:{createElement:()=>({})}});
assert.equal(output[0].href,'/Profiles/Details/TEST0001');assert.equal(output[0].textContent,'TEST0001');assert(!code.includes('employee-quality-dot'));
assert(!fs.readFileSync('Views/Dashboard/Index.cshtml','utf8').includes('_EmployeeQualityLegend'));
assert(fs.readFileSync('Views/Shared/Grid.cshtml','utf8').includes('_EmployeeQualityDot'));
console.log('PASS Overview has a plain employee link and no dot/legend; employee catalog still retains its status dots.');
