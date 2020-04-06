const WebSocket = require('ws');

class Server{
    constructor(port){
        this.clients = []
        this.ws = new WebSocket.Server({port:port})
        this.ws.on('connection',this.connentionListener.bind(this))
        p(`server running at ${port}`)
        this.GotFirstTicket = false
        this.TicketX = 0
        this.TicketY = 0
        this.StartGame = false
        this.GameTime = 120
        this.EndTime_M = 0
        this.EndTime_S = 0
    }
    
    // Initialize client while connecting
    connentionListener(ws,request){
        // return client if it is ready
        this.clients = this.clients.filter(c=>c.readyState === 1)
        
        ws.name = ws._socket.remoteAddress + ":" + `${Math.random()}`.slice(2, 14)
        this.clients.push(ws)
        
        this.SingleSend(ws,JSON.stringify({type:'first'}))
        p(`${ws.name} Login `)
        ws.on('message',data=>{
            let d = JSON.parse(data)
            let detailData
            if(d.Type !== 'StartGame') detailData = JSON.parse(d.Data)
            switch(d.Type){
                case 'Update':
                break;
                case 'Login':
                for(let c of this.clients){
                    if(c.Name == detailData.Data.Name){
                        this.SingleSend(ws,JSON.stringify({
                            Type : "LoginFail",
                        }))
                        return
                    }
                }
                ws.name = detailData.Data.Name
                p(`${detailData.Data.Name} Login `)
                p(this.clients.indexOf(ws))
                this.clients[this.clients.indexOf(ws)].Name =  detailData.Data.Name
                this.clients[this.clients.indexOf(ws)].Character =  detailData.Data.Character
                this.clients[this.clients.indexOf(ws)].HavingTicket = false
                this.clients[this.clients.indexOf(ws)].Turn = false
                this.clients[this.clients.indexOf(ws)].KickCnt = 0
                var userList=[]
                for(let c of this.clients){
                    var usersData={}
                    usersData.Name = c.Name
                    usersData.X = c.X
                    usersData.Y = c.Y
                    usersData.Turn = c.Turn
                    usersData.Character = c.Character
                    usersData.HavingTicket = c.HavingTicket
                    userList.push(usersData)
                }
                var remainingTime
                if(this.StartGame){
                    var now = new Date()
                    p(this.EndTime_M - now.getMinutes())
                    remainingTime = (this.EndTime_M - now.getMinutes()) * 60 + (this.EndTime_S - now.getSeconds())
                    p(`remaining second ${remainingTime}`)
                }
                this.SingleSend(ws,JSON.stringify({
                    Type : "Login",
                    Data : JSON.stringify({ Players: userList , remainingTime : remainingTime})
                }))
                this.boradcaseWithoutMyself(ws,JSON.stringify({
                    Type : "Join",
                    Data : JSON.stringify({ Data : detailData.Data})
                }))
                if(this.clients.length===1){
                    p("born ticket")
                    var bornticketData={}
                    bornticketData.X = Math.random() * 12 + (-6)
                    bornticketData.Y = Math.random() * 8 + (-4)
                    bornticketData.FromPlayer = false
                    this.GotFirstTicket = false
                    this.TicketX = bornticketData.X
                    this.TicketY = bornticketData.Y
                    // p(bornticketData.X)
                    // p(bornticketData.Y)
                    this.broadcast(ws,JSON.stringify({Type : "BornTicket",Data : JSON.stringify({ Data : bornticketData})}))
                }
                else if(!this.GotFirstTicket){
                    var FirstTicketMessage = {}
                    FirstTicketMessage.X = this.TicketX
                    FirstTicketMessage.Y = this.TicketY
                    this.SingleSend(ws,JSON.stringify({
                        Type : "FirstTicket",
                        Data : JSON.stringify({ TicketData: FirstTicketMessage})
                    }))
                }
                break;
                case 'Act':
                this.clients[this.clients.indexOf(ws)].X = detailData.Data.X
                this.clients[this.clients.indexOf(ws)].Y = detailData.Data.Y
                this.clients[this.clients.indexOf(ws)].Turn = detailData.Data.Turn
                // p(detailData.Data.Control)
                if(detailData.Data.Control != 3){
                    this.boradcaseWithoutMyself(ws,data);
                    // p("broad to others")
                }    
                else {
                    this.broadcast(ws,data)
                    // p("broad to all")
                }
                break;
                case 'GotDamage':
                for(let c of this.clients){
                    if(c.Name == detailData.Data.Name && c.HavingTicket) c.HavingTicket = false
                    if(c.Name == detailData.Data.KickerName) c.KickCnt += 1
                }
                this.broadcast(ws,data);
                break;
                case 'BornTicket':
                this.broadcast(ws,data)
                break;
                case 'GetTicket':
                this.GotFirstTicket = true
                for(let c of this.clients){
                    if(c.Name === detailData.Data.Name) c.HavingTicket = true
                    else c.HavingTicket = false
                }
                this.broadcast(ws,data)
                break;
                case 'StartGame':
                var now = new Date()
                this.EndTime_M = Number(now.getMinutes()) + 2
                this.EndTime_S = Number(now.getSeconds()) + 3
                p(`End at ${this.EndTime_M} : ${this.EndTime_S}`)
                this.StartGame = true
                this.GotFirstTicket = false
                for(let c of this.clients){
                    c.X = Math.random() * 12 + (-6)
                    c.Y = Math.random() * 8 + (-4)
                    c.HavingTicket = false
                }
                var userList=[]
                for(let c of this.clients){
                    var usersData={}
                    usersData.Name = c.Name
                    usersData.X = c.X
                    usersData.Y = c.Y
                    usersData.Turn = c.Turn
                    usersData.Character = c.Character
                    usersData.HavingTicket = c.HavingTicket
                    userList.push(usersData)
                }
                var bornticketData={}
                bornticketData.X = Math.random() * 12 + (-6)
                bornticketData.Y = Math.random() * 8 + (-4)
                bornticketData.FromPlayer = false
                this.TicketX = bornticketData.X
                this.TicketY = bornticketData.Y
                this.broadcast(ws,JSON.stringify({
                    Type : "StartGame",
                    Data : JSON.stringify({ Data: { PlayerData : userList, TicketData : bornticketData , RemainingTime : this.GameTime}})
                }))
                
                setTimeout(this.timeCount.bind(this), (this.GameTime+3)*1000)
                break;
                default:
                this.broadcast(ws,data)
                break;
            }
        })
        
        ws.on('close',()=>{
            if(this.clients[this.clients.indexOf(ws)].HavingTicket){
                p("Having Ticket")
                var bornticketData={}
                bornticketData.X = Math.random() * 12 + (-6)
                bornticketData.Y = Math.random() * 8 + (-4)
                bornticketData.FromPlayer = false
                this.GotFirstTicket = false
                this.TicketX = bornticketData.X
                this.TicketY = bornticketData.Y
                this.broadcast(ws,JSON.stringify({Type : "BornTicket",Data : JSON.stringify({ Data : bornticketData})}))
            }
            this.clients.splice(this.clients.indexOf(ws),1)
            this.broadcast(ws,JSON.stringify({
                Type : 'Exit',
                Data : JSON.stringify({
                    Data : {Name : ws.name}
                })
            }))
            p(`${ws.name} exit`)
        })
    }
    
    broadcast(sender,message){
        for(let c of this.clients){
            if(c.readyState === 1) c.send(message)
        }
    }
    
    SingleSend(ws,data){
        ws.send(data)
    }
    
    boradcaseWithoutMyself(sender,message){
        for(let c of this.clients){
            if(c.readyState === 1 && c.name!==sender.name){
                c.send(message)
            } 
        }
    }
    
    timeCount(){
        p(`time up`)
        this.StartGame = false
        var winnername = ""
        var kickCnt = 0
        for(let c of this.clients){
            if(c.HavingTicket) {
                winnername = c.Name
                kickCnt = c.KickCnt
            }
        }
        if(winnername == "") winnername = "N;O:N-E,"
        this.broadcast(null,JSON.stringify({
            Type : 'GameEnd',
            Data : JSON.stringify({
                Data : {WinnerName : winnername,KickCnt : kickCnt}
            })
        }))
    }
    
    sendPosition(){

    }

}

function p(message) {
    process.stdout.write(message + '\n')
}

module.exports = Server