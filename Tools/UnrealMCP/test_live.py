import asyncio,json,sys
from pathlib import Path
from mcp import ClientSession, StdioServerParameters
from mcp.client.stdio import stdio_client
async def main():
 p=StdioServerParameters(command=sys.executable,args=[str(Path(__file__).with_name('server.py'))])
 async with stdio_client(p) as streams:
  async with ClientSession(*streams) as s:
   await s.initialize()
   initial=await s.call_tool('rishi_list_actors',{})
   assert not initial.isError,initial
   initial_data=json.loads(initial.content[0].text)
   created=await s.call_tool('rishi_spawn_actor',{'kind':'cube','label':'MCP_ConnectionVerified','x':0,'y':0,'z':150})
   assert not created.isError,created
   actor=json.loads(created.content[0].text)
   moved=await s.call_tool('rishi_set_actor_location',{'actor_path':actor['path'],'x':200,'y':0,'z':150})
   assert not moved.isError,moved
   assert json.loads(moved.content[0].text)['location']==[200,0,150]
   invalid=await s.call_tool('rishi_spawn_actor',{'kind':'arbitrary_code','label':'Rejected'})
   assert invalid.isError
   protected=next((a for a in initial_data['actors'] if not a['bridge_owned']),None)
   protected_rejected=None
   if protected:
    result=await s.call_tool('rishi_set_actor_location',{'actor_path':protected['path'],'x':0,'y':0,'z':0})
    protected_rejected=result.isError
    assert protected_rejected
   print(json.dumps({'live_spawn':True,'live_move':True,'unsupported_kind_rejected':True,'original_actor_modification_rejected':protected_rejected,'created_actor':actor['label'],'saved':False},indent=2))
asyncio.run(main())
