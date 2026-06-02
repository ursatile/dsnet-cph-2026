import grpc
import price_pb2
import price_pb2_grpc

with grpc.insecure_channel("localhost:50051") as channel:
    stub = price_pb2_grpc.PricerStub(channel)
    reply = stub.GetPrice(price_pb2.PriceRequest(
        make="Volkswagen", model="Golf", year=2021, color="Blue"
    ))
    print(f"{reply.price} {reply.currencyCode}")