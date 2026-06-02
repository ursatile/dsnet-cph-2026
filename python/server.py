import logging
import random
from concurrent import futures

import grpc

import price_pb2
import price_pb2_grpc

class Pricer(price_pb2_grpc.PricerServicer):
    def GetPrice(self, request, context):
        logging.info(
            "Pricing request: %s %s %s (%s)",
            request.year, request.make, request.model, request.color,
        )
        price = random.randint(1000, 50000)
        return price_pb2.PriceReply(price=price, currencyCode="GBP")

def serve(port: int = 5002) -> None:
    server = grpc.server(futures.ThreadPoolExecutor(max_workers=10))
    price_pb2_grpc.add_PricerServicer_to_server(Pricer(), server)
    server.add_insecure_port(f"[::]:{port}")
    server.start()
    logging.info("Pricer gRPC server listening on port %d", port)
    server.wait_for_termination()

if __name__ == "__main__":
    print("Starting pricer gRPC server...")
    logging.info("Starting pricer gRPC server")
    logging.basicConfig(level=logging.INFO, format="%(asctime)s %(levelname)s %(message)s")
    serve()
