const fs=require('node:fs'),vm=require('node:vm'),assert=require('node:assert/strict');
let passed=0;function check(ok,name){assert.ok(ok,name);passed++;console.log('PASS '+name);}
function fixture(div,dep){
 const events={},division={value:div,addEventListener:(n,f)=>events[n]=f};
 const options=[{value:'',dataset:{}},{value:'11',dataset:{divisionId:'1'}},{value:'21',dataset:{divisionId:'2'}}];
 const department={options,focus(){this.focused=true;},get value(){return options.find(o=>o.selected)?.value??'';},set value(v){options.forEach(o=>o.selected=o.value===v);},get selectedOptions(){return options.filter(o=>o.selected);}};department.value=dep;
 const notice={hidden:true,textContent:''},form={querySelector:s=>s==='[name="DivisionId"]'?division:s==='[name="DepartmentId"]'?department:notice,addEventListener:(n,f)=>events['form-'+n]=f};
 vm.runInNewContext(fs.readFileSync('wwwroot/js/organization-filter.js','utf8'),{document:{querySelectorAll:()=>[form]}});return {events,division,department,notice,options};
}
let f=fixture('1','21');check(f.department.value===''&&!f.notice.hidden,'incompatible bookmarked selection cleared with explanation');check(f.options[2].hidden&&f.options[2].disabled,'other division department unavailable');
f=fixture('1','11');check(f.department.value==='11'&&f.notice.hidden,'valid initial context retained');f.division.value='2';f.events.change();check(f.department.value===''&&!f.notice.hidden,'changing division clears old department explicitly');check(!f.options[2].disabled&&f.options[1].disabled,'new division exposes matching choices only');
f.department.value='21';f.division.value='';f.events.change();check(f.department.value==='21'&&f.options.every(o=>!o.disabled),'clearing division retains valid department and restores choices');
f.division.value='1';f.department.value='21';let prevented=false;f.events['form-submit']({preventDefault(){prevented=true;}});check(prevented&&f.department.focused,'submit rejects injected incompatible UI combination');
console.log(`${passed} dependent filter checks passed (simulated DOM).`);
