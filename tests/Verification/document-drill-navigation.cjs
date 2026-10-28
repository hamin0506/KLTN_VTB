// Browser-independent navigation test; no business data and no real HTTP requests.
const fs=require('fs'),vm=require('vm'),assert=require('assert');
const nodes=new Map();function node(id){if(!nodes.has(id))nodes.set(id,{dataset:{},listeners:{},value:'',textContent:'',hidden:false,disabled:false,addEventListener(name,fn){this.listeners[name]=fn;},replaceChildren(){},querySelectorAll(){return [];}});return nodes.get(id);}
const root=node('document-drill');root.dataset={api:'/Dashboard/DocumentDetailsData',back:'/Dashboard'};
const query='?state=DA_HET_HAN&DivisionIds=2&DepartmentIds=3&DepartmentIds=4&Statuses=DangLamViec&overviewSearch=Nguyen&overviewPage=3&overviewPageSize=50&overviewSort=name&overviewDesc=false';
let requests=[];const ctx={URLSearchParams,AbortController,location:{search:query,pathname:'/Dashboard/DocumentDetails'},history:{replaceState(){}},document:{getElementById:node},window:{addEventListener(){}},fetch:async url=>{requests.push(url);return {ok:false,status:503,json:async()=>({message:'SQL unavailable'})};}};
vm.runInNewContext(fs.readFileSync('wwwroot/js/document-drill.js','utf8'),ctx);
setImmediate(async()=>{
 const back=new URL('http://localhost'+node('drill-back').href);
 assert.deepEqual(back.searchParams.getAll('DepartmentIds'),['3','4']);assert.equal(back.searchParams.get('Statuses'),'DangLamViec');assert.equal(back.searchParams.get('page'),'3');assert.equal(back.searchParams.get('pageSize'),'50');assert.equal(back.searchParams.get('sort'),'name');assert.equal(back.searchParams.get('desc'),'false');assert.equal(back.searchParams.get('search'),'Nguyen');assert.equal(back.searchParams.has('state'),false);
 assert.equal(new URL('http://localhost'+requests[0]).searchParams.get('state'),'DA_HET_HAN');assert.equal(node('drill-error').textContent,'SQL unavailable');assert.equal(node('drill-error').hidden,false);
 node('drill-term').value='employee search';node('drill-search').listeners.submit({preventDefault(){}});
 await new Promise(resolve=>setImmediate(resolve));const next=new URL('http://localhost'+requests[1]);assert.equal(next.searchParams.get('search'),'employee search');assert.equal(next.searchParams.get('page'),'1');assert.equal(next.searchParams.get('overviewPage'),'3');assert.deepEqual(next.searchParams.getAll('DepartmentIds'),['3','4']);
 console.log('PASS: dynamic state, all organization selections, status, return search/sort/page, independent detail search, SQL error UI. No live SQL or browser layout tested.');
});
