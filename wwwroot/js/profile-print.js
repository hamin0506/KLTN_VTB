const printScope=document.getElementById('print-scope');
function applyPrintScope(){document.body.dataset.printScope=printScope?.value||'all';}
printScope?.addEventListener('change',applyPrintScope);
window.addEventListener('beforeprint',applyPrintScope);
document.getElementById('print-profile')?.addEventListener('click',()=>{applyPrintScope();window.print();});
applyPrintScope();
