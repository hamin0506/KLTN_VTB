(() => {
 const root=document.documentElement, sidebar=document.getElementById('app-sidebar'), toggle=document.getElementById('sidebar-toggle');if(!sidebar||!toggle)return;
 const mobileToggle=document.getElementById('sidebar-mobile-toggle');
 const media=window.matchMedia('(min-width:992px)'),overlay=document.querySelector('.sidebar-backdrop'),close=sidebar.querySelector('.sidebar-close'),groups=[...sidebar.querySelectorAll('details.nav-group')];let compact=false;
 try{compact=localStorage.getItem('vtb.sidebar.compact')==='true';}catch{}
 function position(){groups.forEach(g=>g.style.setProperty('--submenu-top',Math.min(g.getBoundingClientRect().top,Math.max(8,window.innerHeight-160))+'px'));}
 function sync(){root.classList.toggle('sidebar-compact',compact);const expanded=media.matches?!compact:root.classList.contains('sidebar-mobile-open');toggle.setAttribute('aria-expanded',String(expanded));toggle.title=toggle.ariaLabel=media.matches?(expanded?'Thu gọn thanh bên':'Mở rộng thanh bên'):(expanded?'Đóng thanh menu':'Mở thanh menu');mobileToggle?.setAttribute('aria-expanded',String(!media.matches&&expanded));overlay.hidden=media.matches||!expanded;sidebar.inert=!media.matches&&!expanded;position();window.dispatchEvent(new Event('resize'));}
 function shut(){root.classList.remove('sidebar-mobile-open');sync();(media.matches?toggle:mobileToggle)?.focus();}
 toggle.addEventListener('click',()=>{if(media.matches){compact=!compact;groups.forEach(g=>{g.open=!compact&&!!g.querySelector('.active')});try{localStorage.setItem('vtb.sidebar.compact',String(compact));}catch{}}else{root.classList.toggle('sidebar-mobile-open');}sync();if(!media.matches&&root.classList.contains('sidebar-mobile-open'))close.focus();});
 mobileToggle?.addEventListener('click',()=>toggle.click());
 close.addEventListener('click',shut);overlay.addEventListener('click',shut);
 document.addEventListener('keydown',e=>{if(e.key==='Escape'){groups.forEach(g=>{if(compact&&media.matches)g.open=false});if(root.classList.contains('sidebar-mobile-open'))shut();}
 if(e.key==='Tab'&&!media.matches&&root.classList.contains('sidebar-mobile-open')){const items=[...sidebar.querySelectorAll('a,button,summary')].filter(x=>x.getClientRects().length);const first=items[0],last=items.at(-1);if(e.shiftKey&&document.activeElement===first){e.preventDefault();last.focus();}else if(!e.shiftKey&&document.activeElement===last){e.preventDefault();first.focus();}}});
 document.addEventListener('click',e=>{if(compact&&media.matches&&!sidebar.contains(e.target))groups.forEach(g=>g.open=false);});
 groups.forEach(g=>g.addEventListener('toggle',position));
 media.addEventListener('change',()=>{root.classList.remove('sidebar-mobile-open');sync();});window.addEventListener('resize',position);
 if(compact&&media.matches)groups.forEach(g=>g.open=false);sync();
})();
