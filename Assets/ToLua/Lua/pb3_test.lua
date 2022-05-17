local pb = require "pb"
local protoc = require "protoc"

-- load schema from text
assert(protoc:load [[
    syntax = "proto3";
    
    message Phone {
        string name = 1;
        int64 phonenumber = 2;
    }
    message Person {
        string name = 1;
        int32 age = 2;
        string address = 3;
        repeated Phone contacts = 4;
    } ]])
    
-- lua table data
local data = {
    name = "ilse",
    age = 18,
    contacts = {
        { name = "alice", phonenumber = 1231234123412345678 },
        --{ name = "alice", phonenumber = 652898023277872107 },
        { name = "bob", phonenumber = 45645674567 }
        --phonenumber = 1.2312341234123456e+018
    }
}

-- encode lua table data into binary format in lua string and return
local bytes = assert(pb.encode("Person", data))
print(pb.tohex(bytes))
-- and decode the binary data back into lua table
local data2 = assert(pb.decode("Person", bytes))
print(require "serpent".block(data2))
print(data2.contacts[1].phonenumber, data2.contacts[1].phonenumber == 1231234123412345678)